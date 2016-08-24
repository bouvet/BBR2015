var baseUrl = "../api/";
var meldingsSekvens = 0;
var map_isVisible = true;
var map = null;
var post_marker_icon_general = null;

var post_color_map = [["red", 95], ["orange", 75], ["yellow", 55],
                    ["green_yellow", 35], ["green", 15], ["gray", 0]];


function updateAndDisplayMapOrMessage(show_map) {
    var bootstrap_size = findBootstrapEnvironment();
    if (bootstrap_size === 'xs') {
        if (show_map) {
            $("#messages")[0].style.display = 'none';
            $("#map")[0].style.display = 'block';

            $("#btn_switch_map_messages_span")[0].innerHTML = "Meldinger";
        } else {
            $("#messages")[0].style.display = 'block';
            $("#map")[0].style.display = 'none';

            $("#btn_switch_map_messages_span")[0].innerHTML = "Map";
        }
    } else {
        //document.getElementById("messages").style.display = 'block';
       $("#messages")[0].style.display = 'block';
       $("#map")[0].style.display = 'block';
       $("#btn_switch_map_messages_span")[0].innerHTML = "Messages";
    }

    updateMapAndMessagesSize();
}

function updateScoreDiffToNextAndPrevTeam(ranking) {
    var prev_team_msg = ranking.poengForanLagetBak + " <span class='glyphicon glyphicon-circle-arrow-up'> </span> ";
    var next_team_msg = " <span class='glyphicon glyphicon-circle-arrow-down'> </span> " + ranking.poengBakLagetForan;

    $("#team_status_prev_team_score_diff_xs")[0].innerHTML = prev_team_msg;
    $("#team_status_next_team_score_diff_xs")[0].innerHTML = next_team_msg;

    $("#team_status_prev_team_score_diff_main")[0].innerHTML = prev_team_msg;
    $("#team_status_next_team_score_diff_main")[0].innerHTML = next_team_msg;
}

function switchMapAndMessages() {
    map_isVisible = !map_isVisible;
    updateAndDisplayMapOrMessage(map_isVisible);
}

function displayNumberOfWeapons(bombs, traps) {
    $("#bomb_label_modal")[0].innerHTML = "x" + bombs;
    $("#trap_label_modal")[0].innerHTML = "x" + traps;

    $("#bomb_label_main")[0].innerHTML = "x" + bombs;
    $("#trap_label_main")[0].innerHTML = "x" + traps;
 
    if (bombs === 0) {
        $("#bomb_btn_main")[0].classList.add("disabled");
        $("#bomb_btn_modal")[0].classList.add("disabled");
    } else {
        $("#bomb_btn_main")[0].classList.remove("disabled");
        $("#bomb_btn_modal")[0].classList.remove("disabled");
    }

    if (traps === 0) {
        $("#trap_btn_main")[0].classList.add("disabled");
        $("#trap_btn_modal")[0].classList.add("disabled");
    } else {
        $("#trap_btn_main")[0].classList.remove("disabled");
        $("#trap_btn_modal")[0].classList.remove("disabled");
    }
}

var post_markers = [];
function updatePostsOnMap(Posts) {
    if (!(map === null)) {
        //clear old post markers
        post_markers.forEach(function (post_marker) {
            map.removeLayer(post_marker);
        });
        post_markers = [];

        //add new post markers
        if (!(Posts === undefined)) { Posts.forEach(putPostOnMap(post)); }
    }
}

function putPostOnMap(post) {
    var lat   = post.Latitude;
    var lon   = post.Longitude;
    var value = post.PoengVerdi;
    var isRegistered = post.HarRegistrert;

    var post_color = "gray";
    if (isRegistered==="false") {
        for (var i = 0; i < post_color_map.length; i++) {
            if (value > post_color_map[i][1]) {
                post_color = post_color_map[i][0];
                break;
            }
        }
    }

    var post_marker_color = new post_marker_icon_general({ iconUrl: "img/flag_"+post_color+".png" });
    var post_marker = L.marker([lat, lon], { icon: post_marker_color }).addTo(map);

    post_marker.bindPopup(""+value);
    post_markers[post_markers.length] = post_marker;
}

var player_markers = [];
function updateTeamOnMap(players) {
    if (!(map === null)) {
        //clear old player markers
        player_markers.forEach(function (player_marker) {
            map.removeLayer(player_marker)
        });
        player_markers = [];

        //add new player markers
        if (!(players === undefined)) {
            players.forEach(function (player) {
                putPlayerOnMap(player)
            });
        }
    }
}

function putPlayerOnMap(player) {
    var lat = player.latitude;
    var lon = player.longitude;
    var name = player.navn;

    var distance_m = calcDistanceToClientPlayer(lat, lon);
    if (distance_m < 12) {
        return;
    }

    player_markers[player_markers.length] = L.circle([lat, lon], 6, {
        color: 'blue',
        fillColor: 'blue',
        fillOpacity: 1,
        weight: 0,
    }).addTo(map);

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
    updatePostsOnMap(gameState.Poster);
    updateScoreDiffToNextAndPrevTeam(gameState.ranking);
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

function getTeamPosition() {
    $.ajax({
        type: 'GET',
        url: baseUrl + 'PosisjonsService',
        headers: createHeader()
    }).success(updateTeamOnMap);
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

function sendMessage(msg, successHandler, errHandler) {
    showToast("Melding sendt");
    $.ajax({
        type: "POST",
        url: baseUrl + 'Meldinger',
        headers:
        createHeader(),
        data: JSON.stringify({ tekst: msg }),
        success: successHandler,
        error : errHandler
    });
};

function registerPost(input) {
    var postId = $("#post_id_" + input)[0].value;
    $("#post_id_" + input)[0].value = "";
    var weapon = "";
    if ($("#bomb_radio_" + input)[0].checked) weapon = "BOMBE";
    if ($("#trap_radio_" + input)[0].checked) weapon = "FELLE";

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

var currentLocation = null;
var player_position = {"lat":0, "lon":0};
var circle = null;
function sendPosition() {
    navigator.geolocation.getCurrentPosition(success, null, { maximumAge: 0, timeout: 5000, enableHighAccuracy: true });
    function success(position) {
        player_position.lat = position.coords.latitude;
        player_position.lon = position.coords.longitude;
        var data = JSON.stringify({
            "latitude": player_position.lat,
            "longitude": player_position.lon
        });

        if (!(map === null)) {
            if (!(circle === null)) {
                map.removeLayer(circle);
            }
            circle = L.circle([player_position.lat, player_position.lon], 6, {
                color: 'red',
                fillColor: '#f03',
                fillOpacity: 1,
                weight: 0
            }).addTo(map);
        }

        $.ajax({
            type: "POST",
            url: baseUrl + 'PosisjonsService',
            headers: createHeader(),
            data: data
        }).success(console.log("Posisjon sendt"));
    }
};

// -----------------------------------
// ---   Save/load user options    ---
// -----------------------------------

function saveUserOption() {
    localStorage.setItem("prevUser", "true");

    var lag_kode = $("#lag_kode")[0].value;
    localStorage.setItem("lag_kode", lag_kode);

    var deltaker_kode = $("#deltaker_kode")[0].value;
    localStorage.setItem("deltaker_kode", deltaker_kode);

    var radio_auto_update_no = $("#radio_auto_update_no")[0].checked;
    var radio_auto_update_yes = $("#radio_auto_update_yes")[0].checked;

    if (radio_auto_update_no === true) {
        localStorage.setItem("auto_update_setting", "false");
    } else if (radio_auto_update_yes === true) {
        localStorage.setItem("auto_update_setting", "true");
    } else {
        alert("error in Loading user options. Autoupdate value is undefined.")
        return;
    }

    logIn(lag_kode, deltaker_kode);
}

function loadUserOptions() {
    var prevUser = localStorage.getItem("prevUser");
    if (prevUser === "true") {
        var lag_kode = localStorage.getItem("lag_kode");
        $("#lag_kode")[0].value = lag_kode;

        var deltaker_kode = localStorage.getItem("deltaker_kode");
        $("#deltaker_kode")[0].value = deltaker_kode;
        
        //logIn(lag_kode, deltaker_kode); disabled for debugging
    } else {
        $('#options_modal')[0].modal('show');
    }
}

function logIn(lag_kode, deltaker_kode) {
    var msg = "Deltaker  " + deltaker_kode + " logget inn";

    var successHandler = function () { showToast("Innlogget med lagkode " + lag_kode + " og deltager id " + deltaker_kode); };
    var errHandler = function () { showToast("Innlogging feilet"); };
    sendMessage(msg, successHandler, errHandler);
}

// -------------------
// ---   Events    ---
// -------------------

//Time-event
setInterval(function () {
    var auto_update = localStorage.getItem("auto_update_setting");
    if (auto_update === "true") {
        sendPosition();
        getGameState();
        updateMessages();
        getTeamPosition();
    }
}, 3000);

//Event that triggers when all of HTML has been loaded
window.onload = function () {
    map = L.map('map').setView([59.935, 10.7585], 15);

    post_marker_icon_general = L.Icon.extend({
        options: {
            iconSize: [30, 42],
            iconAnchor: [10, 42],
            popupAnchor: [0, -42]
        }
    });

    L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    updateAndDisplayMapOrMessage(map_isVisible);
    loadUserOptions();
    updateMessages();
    getGameState();
    sendPosition();
    getTeamPosition();

    $("#btn_switch_map_messages")[0].onclick = function () {
        switchMapAndMessages();
    }

    $("#send_messages")[0].onclick = function () {
        var msg = $("#send_messages_textbox_main")[0].value;
        $("#send_messages_textbox_main")[0].value = "";
        sendMessage(msg);
    }

    $("#register_post_btn_main")[0].onclick = function () {
        registerPost("main");
    }
    $("#register_post_btn_modal")[0].onclick = function () {
        registerPost("modal");
    }

    
    $("#registrer_user")[0].onclick = function () {
        saveUserOption();
    }

    $("#bomb_btn_main")[0].onclick = function () {
        if ($("#bomb_btn_main")[0].classList.contains("disabled")) setTimeout(autoSelectNoWeapon, 2);
    }
    $("#trap_btn_main")[0].onclick = function () {
        if ($("#trap_btn_main")[0].classList.contains("disabled")) setTimeout(autoSelectNoWeapon, 2);
    }
    $("#bomb_btn_modal")[0].onclick = function () {
        if ($("#bomb_btn_modal")[0].classList.contains("disabled")) setTimeout(autoSelectNoWeapon, 2);
    }
    $("#trap_btn_modal")[0].onclick = function () {
        if ($("#trap_btn_modal")[0].classList.contains("disabled")) setTimeout(autoSelectNoWeapon, 2);
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
        map_margin_top = 40;
        messages_margin_top = 35;

        map_margin_bottom = 30;
        messages_margin_bottom = 30;

        map_height = height_screen - map_margin_bottom - map_margin_top;
        messages_height = map_height-60;
        messages_inner_panel_height = messages_height;
        
        $("#messages_panel_heading")[0].style.display = 'none';
    } else if (bootstrap_size === 'sm') {
        messages_margin_top = 23;
        messages_margin_left = 10;
        messages_margin_right = 50;
        map_margin_left = 10;

        map_height = $('#register_post_panel').height() + $('#options_and_information').height() + 22;
        console.log(map_height);
        messages_height = height_screen - map_height - 50;
        messages_inner_panel_height = messages_height - 100;

        $("#messages_panel_heading")[0].style.display = 'block';
    } else {
        map_margin_bottom = 35;
        map_margin_right = 10;
        map_margin_left = 10;
        map_height = height_screen - map_margin_bottom - map_margin_top;

        messages_height = map_height;
        messages_inner_panel_height = messages_height - 100;

        $("#messages_panel_heading")[0].style.display = 'block';
    }
    
    $("#map")[0].style.height = map_height + 'px';
    $("#map")[0].style.marginTop = map_margin_top + 'px';
    $("#map")[0].style.marginLeft = map_margin_left + 'px';
    $("#map")[0].style.marginRight = map_margin_right + 'px';

    $("#messages")[0].style.height = messages_height + 'px';
    $("#messages")[0].style.marginTop = messages_margin_top + 'px';
    $("#messages")[0].style.marginLeft = messages_margin_left + 'px';
    $("#messages")[0].style.marginRight = messages_margin_right + 'px';

    $("#messages_panel_body")[0].style.height = messages_inner_panel_height + 'px';
    $("#information_modal_body")[0].style.height = (height_screen - 250) + 'px';
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
    $("#no_weapon_radio_main")[0].checked = "true";
    $("#no_weapon_btn_main")[0].classList.add("active");
    //document.getElementById("no_weapon_btn_main").focus();
    $("#bomb_btn_main")[0].classList.remove("active");
    $("#trap_btn_main")[0].classList.remove("active");

    $("#no_weapon_radio_modal")[0].checked = "true";
    $("#no_weapon_btn_modal")[0].classList.add("active");
    //document.getElementById("no_weapon_btn_modal").focus();
    $("#bomb_btn_modal")[0].classList.remove("active");
    $("#trap_btn_modal")[0].classList.remove("active");
}

// should work for small distances #siving
function calcDistanceToClientPlayer(lat, lon) {
    var deltaLat = lat - player_position.lat;
    var deltaLon = lon - player_position.lon;

    var earthCircumference_m = 40000;
    var deltaLat_m = deltaLat * earthCircumference_m * Math.cos(Math.PI / 180 * lat);
    var detlaLon_m = deltaLon * earthCircumference_m;
    var distance = Math.sqrt(Math.pow(deltaLat_m, 2) + Math.pow(detlaLon_m, 2));
    return distance;
}