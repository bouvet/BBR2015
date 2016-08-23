var baseUrl = "../api/";
var meldingsSekvens = 0;
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

function displayNumberOfWeapons(bombs, traps) {
    document.getElementById("bomb_label_modal").innerHTML = "x" + bombs;
    document.getElementById("trap_label_modal").innerHTML = "x" + traps;

    document.getElementById("bomb_label_main").innerHTML = "x" + bombs;
    document.getElementById("trap_label_main").innerHTML = "x" + traps;
 
    if (bombs === 0) {
        document.getElementById("bomb_btn_main").classList.add("disabled");
        document.getElementById("bomb_btn_modal").classList.add("disabled");
    } else {
        document.getElementById("bomb_btn_main").classList.remove("disabled");
        document.getElementById("bomb_btn_modal").classList.remove("disabled");
    }

    if (traps === 0) {
        document.getElementById("trap_btn_main").classList.add("disabled");
        document.getElementById("trap_btn_modal").classList.add("disabled");
    } else {
        document.getElementById("trap_btn_main").classList.remove("disabled");
        document.getElementById("trap_btn_modal").classList.remove("disabled");
    }
    //autoSelectNoWeapon();
}

// ------------------------------------------------
// --- Recive data from server (post/messages)  ---
// ------------------------------------------------

function updateMessages() {
    $.ajax({
        type: "GET",
        url: baseUrl + 'Meldinger/' + meldingsSekvens,
        headers: createHeader()
    }).done(displayMessages);
}

function displayMessages(data) {
    data.meldinger.reverse();
    data.meldinger.forEach(function (melding) {
        if (meldingsSekvens < melding.sekvens) {
            meldingsSekvens = melding.sekvens;
        }
        $("#messages_list").prepend(
          "<li class='list-striped'>" +
            "<div>" + melding.deltaker + "</div>" +
            "<div>" + melding.melding + "</div>" +
          "</li>"
          );
    });
};

function getGameState() {
    $.ajax({
        type: 'GET',
        url: baseUrl + 'GameStateFeed',
        headers: createHeader()
    }).success(processGameState);
};

function processGameState(gameState) {
    weaponsAviable(gameState.vaapen)
}

function weaponsAviable(weapons) {
    var n_bombs = 0;
    var n_traps = 0;
    weapons.forEach(function (weapon) { 
        switch(weapon.vaapenId){
            case "BOMBE":
                n_bombs++;
                break;
            case "FELLE":
                n_traps++;
                break;
        }
    });
    displayNumberOfWeapons(n_bombs, n_traps);
}

// ----------------------------------------------
// ---   Send data to server (post/messages)  ---
// ----------------------------------------------

 function createHeader() {
     var lag_kode = localStorage.getItem("lag_kode");
     var deltaker_kode = localStorage.getItem("deltaker_kode");

    headers = {
        "Content-Type": "application/json",
        "LagKode": lag_kode,
        "DeltakerKode": deltaker_kode
    };
    return headers;
};

function sendMessage(msg) {
    showToast("Melding sendt");
    $.ajax({
        type: "POST",
        url: baseUrl + 'Meldinger',
        headers:
        createHeader(),
        data: JSON.stringify({tekst : msg})
    });
};

function registerPost(postId, weapon) {
    console.log(postId);
    console.log(weapon);

    if (weapon === "") {
        weapon = null;
    }

    $.ajax({
        type: "POST",
        url: baseUrl + 'GameService',
        headers:
        createHeader(),
        data: JSON.stringify({
            'postKode': postId,
            'våpen': weapon
        })
    });
    autoSelectNoWeapon();
    showToast("Post Registrert");
};

// -----------------------------------
// ---   Save/load user options    ---
// -----------------------------------

function saveUserOption() {
    localStorage.setItem("prevUser", "true");

    var lag_kode = document.getElementById("lag_kode").value;
    localStorage.setItem("lag_kode", lag_kode);

    var deltaker_kode = document.getElementById("deltaker_kode").value;
    localStorage.setItem("deltaker_kode", deltaker_kode);

    var radio_auto_update_no = document.getElementById("radio_auto_update_no").checked;
    var radio_auto_update_yes = document.getElementById("radio_auto_update_yes").checked;

    if (radio_auto_update_no === true) {
        localStorage.setItem("auto_update_setting", "false");
    } else if (radio_auto_update_yes === true) {
        localStorage.setItem("auto_update_setting", "true");
    } else {
        alert("error in Loading user options. Autoupdate value is undefined.")
        return;
    }

    showToast("Lagrer brukerinnstillinger... ");
}

function loadUserOptions() {
    var prevUser = localStorage.getItem("prevUser");
    if (prevUser === "true") {
        var lag_kode = localStorage.getItem("lag_kode");
        document.getElementById("lag_kode").value = lag_kode;

        var deltaker_kode = localStorage.getItem("deltaker_kode");
        document.getElementById("deltaker_kode").value = deltaker_kode;
        
        showToast("Lag kode: " + lag_kode + ", deltager kode: "+deltaker_kode);
    } else {
        $('#options_modal').modal('show');
    }
}

// -------------------
// ---   Events    ---
// -------------------

//Time-event
setInterval(function () {
    var auto_update = localStorage.getItem("auto_update_setting");
    if (auto_update === "true") {
        //sendPosisjon();
        getGameState();
        updateMessages();
        //hentLagposisjoner();
    }
}, 3000);

//Event that triggers when all of HTML has been loaded
window.onload = function () {
    updateAndDisplayMapOrMessage(map_isVisible);
    loadUserOptions();
    updateMessages();
    getGameState();

    document.getElementById("btn_switch_map_messages").onclick = function() {
        switchMapAndMessages();
    }

    document.getElementById("send_messages").onclick = function() {
        var msg = document.getElementById("send_messages_textbox_main").value;
        document.getElementById("send_messages_textbox_main").value = "";
        sendMessage(msg);
    }

    document.getElementById("register_post_main").onclick = function () {
        var postId = document.getElementById("post_id_main").value;
        document.getElementById("post_id_main").value = "";
        var weapon = "";
        if (document.getElementById("bomb_radio_main").checked) weapon = "BOMBE";
        if (document.getElementById("trap_radio_main").checked) weapon = "FELLE";

        registerPost(postId, weapon);
    }

    document.getElementById("register_post_modal").onclick = function () {

    }

    
    document.getElementById("registrer_user").onclick = function() {
        saveUserOption();
    }

    var map = L.map('map').setView([59.935, 10.7585], 15);

    L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    var marker = L.marker([59.935, 10.7585]).addTo(map);
    map.removeLayer(marker);

    document.getElementById("bomb_btn_main").onclick = function () {
        if (document.getElementById("bomb_btn_main").classList.contains("disabled")) setTimeout(autoSelectNoWeapon, 2);
    }
    document.getElementById("trap_btn_main").onclick = function () {
        if (document.getElementById("trap_btn_main").classList.contains("disabled")) setTimeout(autoSelectNoWeapon, 2);
    }
    document.getElementById("bomb_btn_modal").onclick = function () {
        if (document.getElementById("bomb_btn_modal").classList.contains("disabled")) setTimeout(autoSelectNoWeapon, 2);
    }
    document.getElementById("trap_btn_modal").onclick = function () {
        if (document.getElementById("trap_btn_modal").classList.contains("disabled")) setTimeout(autoSelectNoWeapon, 2);
    }
}

//Event that triggers when the screen changes size
$(window).resize(function () {
    updateAndDisplayMapOrMessage(map_isVisible);
}).resize()


// -------------------
// ---   Helpers   ---
// -------------------

function showToast(msg) {
    var x = document.getElementById("snackbar")
    x.innerHTML = msg;
    x.className = "show";
    setTimeout(function () { x.className = x.className.replace("show", ""); }, 3000);
}

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

function autoSelectNoWeapon() {
    document.getElementById("no_weapon_radio_main").checked = "true";
    document.getElementById("no_weapon_btn_main").classList.add("active");
    //document.getElementById("no_weapon_btn_main").focus();
    document.getElementById("bomb_btn_main").classList.remove("active");
    document.getElementById("trap_btn_main").classList.remove("active");

    document.getElementById("no_weapon_radio_modal").checked = "true";
    document.getElementById("no_weapon_btn_modal").classList.add("active");
    //document.getElementById("no_weapon_btn_modal").focus();
    document.getElementById("bomb_btn_modal").classList.remove("active");
    document.getElementById("trap_btn_modal").classList.remove("active");
}