$(function () {

    //$('#chatBody').hide();
    //$('#loginBlock').show();

    // Ссылка на автоматически-сгенерированный прокси хаба
    var chat = $.connection.chatHub;
    // Объявление функции, которая хаб вызывает при получении сообщений
    chat.client.addMessage = function (name, message) {
        // Добавление сообщений на веб-страницу 
        $('#inboxMssageArea').append('<p><b>' + htmlEncode(name)
            + '</b>: ' + htmlEncode(message) + '</p>');
    };

    // Функция, вызываемая при подключении нового пользователя
    chat.client.onConnected = function (id, userName, allUsers) {

        // установка в скрытых полях имени и id текущего пользователя
        $('#hdId').val(id);

        // Добавление всех пользователей
        for (i = 0; i < allUsers.length; i++) {

            AddUser(allUsers[i].ConnectionId, allUsers[i].Name);
        }
    };

    // Добавляем нового пользователя
    chat.client.onNewUserConnected = function (id, name) {

        AddUser(id, name);
    };

    // Удаляем пользователя
    chat.client.onUserDisconnected = function (id, userName) {

        $('#' + id).remove();
    };

    // Открываем соединение
    $.connection.hub.start().done(function () {

        $('#sendAll').click(function () {
            var msg = $('#message').val();
            try{
                chat.server.broadcast( msg );
            } catch (e) {
                chat.server.connect();
                chat.server.broadcast( msg);
            }
            $('#message').val('');
        });

        $('#sendSelected').click(function () {
            var users = GetSelectedUsers();
            if (users.length === 0) {
                alert("Select any users!");
                return;
            }
            var msg = $('#message').val();
            try {
                chat.server.send(users.users, msg);
            } catch (e) {
                chat.server.connect();
                chat.server.send(users.users, msg);
            }
            var name = users.names.join(", ");
            $('#inboxMssageArea').append('<p class="out-message"><b> To ' + name
            + '</b>: ' + msg + '</p>');

            $('#message').val('');
        });
        chat.server.connect();
    });
});
// Кодирование тегов
function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}
//Добавление нового пользователя
function AddUser(id, name) {

    var userId = $('#hdId').val();

    if (userId !== id) {
        $("#users").append('<li id="' + id + '"><input type="checkbox" value="' + id + '"/>' + name + '</li>');
    }
}

function GetSelectedUsers() {
    var users = [];
    var names = [];
    $("#users input:checked")
        .each(function () {
            var $t = $(this);
            users.push($t.val());
            names.push($t.parent().text());
        });
    return { users : users, names: names };
}
