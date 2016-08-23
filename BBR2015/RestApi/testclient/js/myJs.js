var map_isVisible = true;

function updateAndDisplayMapOrMessage(show_map) {
    var bootstrap_size = findBootstrapEnvironment();
    if (bootstrap_size === 'xs') {
        if (show_map) {
            document.getElementById("messages").style.display = 'none';
            document.getElementById("map").style.display = 'block';

            document.getElementById("btn_switch_map_messages_span").innerHTML = "Messages";
        } else {
            document.getElementById("messages").style.display = 'block';
            document.getElementById("map").style.display = 'none';

            document.getElementById("btn_switch_map_messages_span").innerHTML = "Map";
        }
    } else {
        document.getElementById("messages").style.display = 'block';
        document.getElementById("map").style.display = 'block';
        document.getElementById("btn_switch_map_messages_span").innerHTML = "Messages";
    }

    updateMapAndMessagesSize();
}

function switchMapAndMessages() {
    map_isVisible = !map_isVisible;
    updateAndDisplayMapOrMessage(map_isVisible);
}

function saveUserOption() {
    localStorage.setItem("prevUser", '1');

    var lag_kode = document.getElementById("lag_kode").value;
    localStorage.setItem("lag_kode", lag_kode);

    var deltaker_kode = document.getElementById("deltaker_kode").value;
    localStorage.setItem("deltaker_kode", deltaker_kode);

    showToast("Lagrer brukerinnstillinger... ");
}

function loadUserOptions() {
    var prevUser = localStorage.getItem("prevUser");
    if (prevUser === '1') {
        var lag_kode = localStorage.getItem("lag_kode");
        document.getElementById("lag_kode").value = lag_kode;

        var deltaker_kode = localStorage.getItem("deltaker_kode");
        document.getElementById("deltaker_kode").value = deltaker_kode;
        showToast("Lag kode: " + lag_kode + ", deltager kode: "+deltaker_kode);
    } else {
        $('#options_modal').modal('show');
    }
}

function showToast(msg) {
    var x = document.getElementById("snackbar")
    x.innerHTML = msg;
    x.className = "show";
    setTimeout(function () { x.className = x.className.replace("show", ""); }, 3000);
}

// -------------------
// ---   Events    ---
// -------------------

//Event that triggers when all of HTML has been loaded
window.onload = function () {
    updateAndDisplayMapOrMessage(map_isVisible);
    document.getElementById("btn_switch_map_messages").onclick = function fun() {
        switchMapAndMessages();
    }

    loadUserOptions();
    document.getElementById("registrer_user").onclick = function fun() {
        saveUserOption();
    }
    

    var map = L.map('map').setView([59.935, 10.7585], 15);

    L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    var marker = L.marker([59.935, 10.7585]).addTo(map);
    map.removeLayer(marker);
}

//Event that triggers when the screen changes size
$(window).resize(function () {
    updateAndDisplayMapOrMessage(map_isVisible);
}).resize()


// -------------------
// ---   Helpers   ---
// -------------------

function updateMapAndMessagesSize() {
    var bootstrap_size = findBootstrapEnvironment();

    var height_screen = $(window).height();
    var map_margin_bottom = 0;
    var map_margin_top = 0;
    var map_margin_right = 0;
    var map_margin_left = 0;
    var map_height = height_screen;

    var messages_margin_bottom = 0;
    var messages_margin_top = 0;
    var messages_margin_right = 0;
    var messages_margin_left = 0;
    var messages_height = height_screen;
    var messages_inner_panel_height = 0;

    if (bootstrap_size === 'xs') {
        map_margin_top = 25;
        messages_margin_top = 20;

        map_margin_bottom = 30;
        messages_margin_bottom = 30;

        map_height = height_screen - map_margin_bottom - map_margin_top;
        messages_height = map_height-60;
        messages_inner_panel_height = messages_height;
        
        document.getElementById("messages_panel_heading").style.display = 'none';
    } else if (bootstrap_size === 'sm') {
        messages_margin_top = 23;
        messages_margin_left = 10;
        map_margin_left = 10;

        map_height = $('#register_post_panel').height() + $('#options_and_information').height() + 22;
        messages_height = height_screen - map_height - 50;
        messages_inner_panel_height = messages_height - 100;

        document.getElementById("messages_panel_heading").style.display = 'block';
    } else {
        map_margin_bottom = 35;
        map_margin_right = 10;
        map_margin_left = 10;
        map_height = height_screen - map_margin_bottom - map_margin_top;

        messages_height = map_height;
        messages_inner_panel_height = messages_height - 100;

        document.getElementById("messages_panel_heading").style.display = 'block';
    }
    
    document.getElementById("map").style.height = map_height + 'px';
    document.getElementById("map").style.marginTop = map_margin_top + 'px';
    document.getElementById("map").style.marginLeft = map_margin_left + 'px';
    document.getElementById("map").style.marginRight = map_margin_right + 'px';

    document.getElementById("messages").style.height = messages_height + 'px';
    document.getElementById("messages").style.marginTop = messages_margin_top + 'px';
    document.getElementById("messages").style.marginLeft = messages_margin_left + 'px';
    document.getElementById("messages").style.marginRight = messages_margin_right + 'px';

    document.getElementById("messages_panel_body").style.height = messages_inner_panel_height + 'px';
    document.getElementById("information_modal_body").style.height = (height_screen-250) + 'px';
}

function findBootstrapEnvironment() {
    var envs = ['xs', 'sm', 'md', 'lg'];

    var $el = $('<div>');
    $el.appendTo($('body'));

    for (var i = envs.length - 1; i >= 0; i--) {
        var env = envs[i];

        $el.addClass('hidden-' + env);
        if ($el.is(':hidden')) {
            $el.remove();
            return env;
        }
    }
}
