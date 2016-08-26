var baseUrl = "../api/";
var meldingsSekvens = 0;
var map_isVisible = true;
var map = null;
var post_marker_icon_general = null;
var prev_rank = -1;
var uleste_meldinger = 0;

var score_next_team_diff = 0;
var score_prev_team_diff = 0;

var post_color_map = [["red", 95], ["orange", 75], ["yellow", 55],
                    ["green_yellow", 35], ["green", 15], ["gray", 0]];

function updateAndDisplayMapOrMessage(show_map) {
    var bootstrap_size = findBootstrapEnvironment();
    if (bootstrap_size === 'xs') {
        if (show_map) {
            $("#messages")[0].style.display = 'none';
            $("#map")[0].style.display = 'block';

            var message_span_msg = uleste_meldinger>0? "Meldinger (" + uleste_meldinger +")":"Meldinger";
            $("#btn_switch_map_messages_span")[0].innerHTML = message_span_msg;
        } else {
            uleste_meldinger = 0;
            $("#messages")[0].style.display = 'block';
            $("#map")[0].style.display = 'none';

            $("#btn_switch_map_messages_span")[0].innerHTML = "Map";
        }
    } else {
        uleste_meldinger = 0;
        //document.getElementById("messages").style.display = 'block';
       $("#messages")[0].style.display = 'block';
       $("#map")[0].style.display = 'block';
       $("#btn_switch_map_messages_span")[0].innerHTML = "Meldinger";
    }

    updateMapAndMessagesSize();
}

function updateScoreDiffToNextAndPrevTeam(ranking) {
    var arrow_up = "<span class='glyphicon glyphicon-circle-arrow-up'> </span>";
    var arrow_down = "<span class='glyphicon glyphicon-circle-arrow-down'> </span>";

    score_next_team_diff = ranking.poengBakLagetForan;
    score_prev_team_diff = ranking.poengForanLagetBak;

    var new_rank = ranking.rank;
    if (new_rank !== prev_rank) {
        var msg = { 'deltaker': "", 'melding': '' };
        
        if (prev_rank === -1) {
            msg.deltaker = 'Ranking';
            msg.melding = 'Dere er på ' + new_rank + '. plass.';
        } else if (new_rank > prev_rank) {
            msg.deltaker = arrow_up + ' Ny ranking ' + arrow_up;
            msg.melding  = 'Rykket frem til ' + new_rank + '. plass!';
        } else {
            msg.deltaker = arrow_down + ' Ny ranking ' + arrow_down;
            msg.melding = 'Falt tilabke til ' + new_rank + '. plass.';
        }

        addNewMessage(msg);
    }
    prev_rank = new_rank;

    var prev_team_msg = score_prev_team_diff + " " + arrow_up + " ";
    var next_team_msg = " " + arrow_down + " " + score_next_team_diff;

    $(".team_status_prev_team_score_diff")[0].innerHTML = prev_team_msg;
    $(".team_status_next_team_score_diff")[0].innerHTML = next_team_msg;

    $(".team_status_prev_team_score_diff")[1].innerHTML = prev_team_msg;
    $(".team_status_next_team_score_diff")[1].innerHTML = next_team_msg;

    $(".team_status_rank")[0].innerHTML = "Rank #" + new_rank;
    $(".team_status_rank")[1].innerHTML = "Rank #" + new_rank;

}

function switchMapAndMessages() {
    map_isVisible = !map_isVisible;
    updateAndDisplayMapOrMessage(map_isVisible);
}

function displayNumberOfWeapons(bombs, traps) {
    $("#bomb_label_modal")[0].innerHTML = "&times;" + bombs;
    $("#trap_label_modal")[0].innerHTML = "&times;" + traps;

    $("#bomb_label_main")[0].innerHTML = "&times;" + bombs;
    $("#trap_label_main")[0].innerHTML = "&times;" + traps;
 
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

function addNewMessage(msg) {
    if (map_isVisible === true) {
        uleste_meldinger++;
        updateAndDisplayMapOrMessage(map_isVisible);
    };
    //$("#last_message_carousel")[0].innerHTML = msg.melding;
    
    $("#messages_list").prepend(
        "<li class='list-striped'>" +
        "<div> <b>" + msg.deltaker + "</b> </div>" +
        "<div>" + msg.melding + "</div>" +
        "</li>"
    );
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
        if (!(Posts === undefined)) {
            Posts.forEach(function (post) {
                putPostOnMap(post);
            });
        }
    }
}

function putPostOnMap(post) {
    var lat   = post.latitude;
    var lon   = post.longitude;
    var value = post.poengVerdi;
    var isRegistered = post.harRegistrert;

    var post_color = "gray";
    if (isRegistered=== 'false') {
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

var players_and_markers = new Map();
function updateTeamOnMap(players) {
    if (!(map === null)) {
        players.forEach(function (player) {
            var player_and_marker = players_and_markers.get(player.navn);
            if (player_and_marker === undefined) { //new player
                console.log("new player: " + player.navn);

                var marker = putPlayerOnMap(player);
                player_and_marker = { 'player': player, 'marker': marker };
                players_and_markers.set(player.navn, player_and_marker);
            } else { // old player
                var lat = (player_and_marker.player.latitude);
                var lon = (player_and_marker.player.longitude);
                var newLatLng = new L.LatLng(lat, lon);
                player_and_marker.marker.setLatLng(newLatLng);
            }
        });
    }
}

var farger = ['red','blue','yellow','orange','black','green']
function putPlayerOnMap(player) {
    var lat = player.latitude;
    var lon = player.longitude;
    var name = player.navn;

    var farge = farger[players_and_markers.size % farger.length];
    var player_marker = L.circle([lat, lon], 6, {
        color: farge,
        fillColor: farge,
        fillOpacity: 1,
        weight: 0,
    }).addTo(map);
    player_marker.bindPopup("" + name);
    return player_marker;
}

// ------------------------------------------------
// --- Recive data from server (post/messages)  ---
// ------------------------------------------------

function updateMessages(successHandler) {
    $.ajax({
        type: "GET",
        url: baseUrl + 'Meldinger/' + meldingsSekvens,
        headers: createHeader(),
        success: successHandler
    }).done(displayMessagesFromServer);
}

function displayMessagesFromServer(data) {
    data.meldinger.reverse();
    data.meldinger.forEach(function (msg) {
        if (meldingsSekvens < msg.sekvens) {
            meldingsSekvens = msg.sekvens;
        }
        addNewMessage(msg);
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
    weaponsAviable(gameState.vaapen);
    //updatePostsOnMap(gameState.poster);

    post = {'latitude': 59.937, 'longitude': 10.7585, 'poengVerdi': 50,'harRegistrert':'false'};
    putPostOnMap(post);

    post2 = { 'latitude': 59.933, 'longitude': 10.7585, 'poengVerdi': 80, 'harRegistrert': 'false' };
    putPostOnMap(post2);


    updateScoreDiffToNextAndPrevTeam(gameState.ranking);
    $(".team_status_score")[0].innerHTML = "Score #" + gameState.score;
    $(".team_status_score")[1].innerHTML = "Score #" + gameState.score;
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

var player_position = {"lat":0, "lon":0};
function sendPosition() {
    navigator.geolocation.getCurrentPosition(success, null, { maximumAge: 0, timeout: 5000, enableHighAccuracy: true });
    function success(position) {
        player_position.lat = position.coords.latitude;
        player_position.lon = position.coords.longitude;
        var data = JSON.stringify({
            "latitude": player_position.lat,
            "longitude": player_position.lon
        });

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
        $('#options_modal').modal('show');
    }
}

function logIn(lag_kode, deltaker_kode) {
    var msg = "Deltaker  " + deltaker_kode + " logget inn";

    var successHandler = function () { showToast("Innlogget med lagkode: " + lag_kode + " og deltagerkode: " + deltaker_kode); };
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

    loadUserOptions();
    updateMessages(getGameState);   // This causes the gamestate to be loaded after the messages. This 
                                    // ensures that the "rank" message from the client appears after
                                    // all the other messages (as this is update in "getGameState"). 
    sendPosition();
    getTeamPosition();

    $("#rank_and_score_Carousel_main").carousel(); 
    $("#rank_and_score_Carousel_sx").carousel();

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
        messages_height = map_height-40;
        messages_inner_panel_height = messages_height;
        
        $("#messages_panel_heading")[0].style.display = 'none';
    } else if (bootstrap_size === 'sm') {
        messages_margin_top = 23;
        messages_margin_left = 10;
        map_margin_left = 10;

        map_height = $('#register_post_panel').height() + $('#options_and_information').height() + 22;
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