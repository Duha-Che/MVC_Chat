using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DAL
{
    public class UserRepository
    {
        private static Lazy<UserRepository> _instanceLazy;

        public static void InitializeInstance( string dbXmlFileName)
        {
            if (!String.IsNullOrEmpty(_fileName))
                throw new Exception("[UserRepository] Instance has been already initialized");

            _fileName = dbXmlFileName;
            CheckFilePath();
            _instanceLazy = new Lazy<UserRepository>(() => new UserRepository());
        }


        public static UserRepository Instance
        {
            get {
                if (_instanceLazy == null)
                    throw new Exception("[UserRepository] Instance his not initialized");
                return  _instanceLazy.Value;
            }
        }

        private static string _fileName = null;

        private XDocument _xml;

        private ConcurrentDictionary<Guid, DbUser> _cache;

        private ConcurrentDictionary<DbUser, EState> _cacheChanges;

        private UserRepository()
        {
            Log.Message("[UserRepository] Starting repository...");

            _cacheChanges = new ConcurrentDictionary<DbUser, EState>();
            _cache = new ConcurrentDictionary<Guid, DbUser>();

            FillCache();

            Log.Message("[UserRepository] Repository started");
        }

        private static void CheckFilePath()
        {
            if (String.IsNullOrWhiteSpace(_fileName) ||! Path.GetExtension(_fileName).Equals(".xml", StringComparison.InvariantCultureIgnoreCase))
            {
                _fileName = null;
                throw new ArgumentException("[UserRepository] File name not set or not proper", "dbXmlFileName");
            }
                    
            if (File.Exists(_fileName))
            {
                Log.Message("[UserRepository] File '{0}' exists", _fileName);
                return;
            }

            Log.Message("[UserRepository] File '{0}' not found", _fileName);

            //Create my own new db file with blackJack and ...
            var aDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(RootTag));
            aDoc.Save(_fileName);
            Log.Message("[UserRepository] File '{0}' created", _fileName);
        }

        private const string RootTag = "root";

        private void FillCache()
        {
            try
            {
                _xml = XDocument.Load(_fileName);
                if (!_xml.Elements().Any())
                {
                    Log.Message("[UserRepository] File '{0}' is empty", _fileName);
                    var aDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), new XElement(RootTag));
                    aDoc.Save(_fileName);
                }

                var count = 0;
                foreach (var each in _xml.Elements().First().Elements())
                {
                    var user = DbUser.DeserializeFromXml(each);
                    _cache.TryAdd(user.Id, user);
                    count++;
                }

                Log.Message("[UserRepository] File '{0}' has {1} records", _fileName, count);
            }
            catch (Exception e)
            {
                Log.Error(e, "[UserRepository] Error on loading data from file '{0}'", _fileName);
                throw;
            }    
        }

        public void UpdateUser(DbUser user)
        {
            bool operationFlag = false;

            _cache.AddOrUpdate(user.Id,
                id =>
                {
                    var state = _cacheChanges.GetOrAdd(user, EState.Insert);
                    if (state == EState.Delete)
                    {
                        _cacheChanges[user] = EState.Update;
                    }
                    operationFlag = true;
                    return user;
                },
                (id, usr) =>
                {
                    var state = _cacheChanges.GetOrAdd(user, EState.Update);
                    if (state == EState.Delete )
                    {
                        _cacheChanges[user] = EState.Update;
                    }
                    operationFlag = false;
                    return user;
                } );

            Log.Message("[UserRepository] User '{0}' {1}", user.Name, operationFlag ? "added" : "updated");
        }

        public void AddUser(DbUser user)
        {
            _cache.AddOrUpdate(
                user.Id,
                id => user,
                (id, usr) =>
                    {
                        throw new Exception(
                            String.Format("User {0} Creation error : Primary key violation. User with key {1} already exists : {2}",
                                user.Name, id, usr.Name));
                    });

            _cacheChanges.AddOrUpdate(user, EState.Insert, (u, s) => s == EState.Delete ? EState.Update : EState.Insert);
            Log.Message("[UserRepository] User '{0}' added", user.Name);
        }

        public DbUser FindByKey(Guid id)
        {
            DbUser result;
            return _cache.TryGetValue(id, out result) ? result : null;
        }

        public DbUser FindByName(String name)
        {
            return _cache.Values.FirstOrDefault(u => u.Name.Equals(name, StringComparison.InvariantCulture));
        }

        public bool  CheckUniqueName(String name)
        {
            return _cache.Values.Any( u => u.Name.Equals(name, StringComparison.InvariantCulture));
        }

        public bool CheckPassword(String name, UInt64 password)
        {
            return _cache.Values.Any(u => u.Password == password && u.Name.Equals(name, StringComparison.InvariantCulture) );
        }

        public bool CheckPassword(String name, string password)
        {
            var hashedPassword = DbUser.HashPassword(password);
            return _cache.Values.Any(u => u.Password == hashedPassword && u.Name.Equals(name, StringComparison.InvariantCulture));
        }

        public void DeleteUser(Guid id)
        {
            DbUser user;
            if (_cache.TryRemove(id, out user))
            {
                _cacheChanges.AddOrUpdate(user, EState.Delete, (u, s) => EState.Delete);
                Log.Message("[UserRepository]User '{0}' deleted", user.Name);
            }
        }

        public void Shutdown()
        {
            Log.Message("[UserRepository] Shudown started...");
            Commit();
            Log.Message("[UserRepository] Shutdown complete");
        }

        public void Commit()
        {
            if (!_cacheChanges.Any())
                return;
            try
            {
                Log.Message("[UserRepository] Save changes started...");
                var toUpdate =
                    _cacheChanges.Where(i => i.Value == EState.Update).Select(i => i.Key).ToList();
                var toDelete =
                    _cacheChanges.Where(i => i.Value == EState.Delete).Select(i => i.Key).ToList();
                var toInsert =
                    _cacheChanges.Where(i => i.Value == EState.Insert).Select(i => i.Key).ToList();

                var root = _xml.Root;
                var documentChanged = false;

                var deleted = 0;
                foreach (var each in toDelete)
                {
                    EState state;
                    if (_cacheChanges.TryRemove(each, out state))
                    {
                        var key = each.Id.ToString();
                        var node = root.Elements()
                            .FirstOrDefault(e => e.Name == DbUser.tagRoot &&
                                                    e.Attribute("id").Value.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                        if (node != null)
                        {
                            node.Remove();
                            documentChanged = true;
                            deleted++;
                        }
                    }
                }

                var updated = 0;
                foreach (var each in toUpdate)
                {
                    EState state;
                    if (_cacheChanges.TryRemove(each, out state))
                    {
                        var key = each.Id.ToString();
                        var node = root.Elements()
                            .FirstOrDefault(e => e.Name == DbUser.tagRoot &&
                                                    e.Attribute("id").Value.Equals(key, StringComparison.InvariantCultureIgnoreCase));
                        if (node != null)
                        {
                            var parent = node.Parent;
                            node.Remove();
                            DbUser.SerializeToXml(each, parent);
                            documentChanged = true;
                            updated++;
                        }
                    }
                }

                var inserted = 0;
                foreach (var each in toInsert)
                {
                    EState state;
                    if (_cacheChanges.TryRemove(each, out state))
                    {
                        DbUser.SerializeToXml(each, root);
                        documentChanged = true;
                        inserted++;
                    }
                }

                if (documentChanged)
                    _xml.Save(_fileName);

                Log.Message("[UserRepository] Save complete : {0} deleted, {1} updated, {2} inserted", deleted, updated, inserted);
            }
            catch (Exception e)
            {
                Log.Error(e, "[UserRepository] Error on committing data to file '{0}'", _fileName);
                throw;
            }
        }

        enum EState { Delete, Insert, Update }
    }
}
