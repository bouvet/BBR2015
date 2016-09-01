var baseUrl = "../api/";
var meldingsSekvens = 0;
var loggedIn = false;
var map_isVisible = true;
var map = null;
var post_marker_icon_general = null;
var prev_rank = -1;
var rank_changed = false;
var rank_msg = "";
var team_score = -1;
var uleste_meldinger = 0;
var hasProcessedGameState = false;

var score_next_team_diff = 0;
var score_prev_team_diff = 0;

var post_color_map = [["red", 195], ["orange", 145], ["green", 85],
                    ["darkgreen", 75], ["gray", 0]];

function updateAndDisplayMapOrMessage(show_map) {
    var bootstrap_size = findBootstrapEnvironment();
    if (bootstrap_size === 'xs') {
        if (show_map) {
            $("#messages")[0].style.display = 'none';
            $("#map")[0].style.display = 'block';

            var message_span_msg = uleste_meldinger > 0 ? "MELDINGER (" + uleste_meldinger + ")" : "MELDINGER";
            $("#btn_switch_map_messages_span")[0].innerHTML = message_span_msg;
        } else {
            uleste_meldinger = 0;
            $("#messages")[0].style.display = 'block';
            $("#map")[0].style.display = 'none';

            $("#btn_switch_map_messages_span")[0].innerHTML = "KART";
        }
    } else {
        uleste_meldinger = 0;
        //document.getElementById("messages").style.display = 'block';
       $("#messages")[0].style.display = 'block';
       $("#map")[0].style.display = 'block';
       $("#btn_switch_map_messages_span")[0].innerHTML = "MELDINGER";
    }

    updateMapAndMessagesSize();
}

function updateScoreDiffToNextAndPrevTeam(ranking) {
    var arrow_up = "<span class='glyphicon glyphicon-circle-arrow-up'> </span>";
    var arrow_down = "<span class='glyphicon glyphicon-circle-arrow-down'> </span>";

    score_next_team_diff = ranking.poengBakLagetForan;
    score_prev_team_diff = ranking.poengForanLagetBak;

    var new_rank = ranking.rank;
    if (new_rank !== prev_rank && prev_rank !== -1) {
        var msg = { 'deltaker': "", 'melding': '' };
        
        if (new_rank < prev_rank) {
            msg.deltaker = arrow_up + ' Ny ranking ' + arrow_up;
            msg.melding  = 'Rykket frem til ' + new_rank + '. plass!';
        } else {
            msg.deltaker = arrow_down + ' Ny ranking ' + arrow_down;
            msg.melding = 'Falt til ' + new_rank + '. plass.';
        }
        rank_msg = msg;
        rank_changed = true;
    }
    prev_rank = new_rank;

    var prev_team_msg = "<b>" + score_prev_team_diff + " " + arrow_up + " </b>";
    var next_team_msg = "<b> " + arrow_down + " " + score_next_team_diff +"</b>";

    $(".team_status_prev_team_score_diff")[0].innerHTML = prev_team_msg;
    $(".team_status_next_team_score_diff")[0].innerHTML = next_team_msg;

    $(".team_status_prev_team_score_diff")[1].innerHTML = prev_team_msg;
    $(".team_status_next_team_score_diff")[1].innerHTML = next_team_msg;

    $(".team_status_rank")[0].innerHTML = "<b>Rank #" + new_rank + "</b>";
    $(".team_status_rank")[1].innerHTML = "<b>Rank #" + new_rank + "</b>";
}

function switchMapAndMessages() {
    map_isVisible = !map_isVisible;
    updateAndDisplayMapOrMessage(map_isVisible);
}

function displayNumberOfWeapons(bombs, traps) {
    var bombs_msg = bombs + "&times;";
    var traps_msg = traps + "&times;";

    $("#bomb_label_modal")[0].innerHTML = bombs_msg;
    $("#trap_label_modal")[0].innerHTML = traps_msg;

    $("#bomb_label_main")[0].innerHTML = bombs_msg;
    $("#trap_label_main")[0].innerHTML = traps_msg;
    
    showOrHideWeaponBtn('bomb', bombs > 0 ? true : false , bombs > 0 ? true : false);
    showOrHideWeaponBtn('trap', traps > 0 ? true : false, traps > 0 ? true : false);
}

function showOrHideWeaponBtn(weapon, active, show) {
    show = active ? show : false;
    var btn = $("#" + weapon + "_btn_modal")[0];
    btn.classList.toggle("disabled", !active);
    btn.classList.toggle("no-pointer-events", !active);
    if(active===false){ autoSelectNoWeapon();}

    if (show) {
        btn.style.display = 'block';
    } else {
        btn.style.display = 'none'; 
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

var postMarkersMap = new Map();
function updatePostsOnMap(postsFromServer) {
    if (map !== null && postsFromServer !== undefined) {
        //Oppdater eller skjul poster
        postsFromServer.forEach(function (postFromServer) {
            var lat = postFromServer.latitude;
            var lon = postFromServer.longitude;
            var idString = "lat:"+lat+";lon:"+lon;
            var postFromMap = postMarkersMap.get(idString);

            if (postFromMap === undefined) { //new post
                var post_marker = putPostOnMap(postFromServer);
                postFromMap = { 'post': postFromServer, 'marker': post_marker, 'inLastServerPostList':true};
                postMarkersMap.set(idString, postFromMap);
                console.log("ny post id: " + idString + ", value: " + postFromServer.poengVerdi)

            }
            postFromMap.inLastServerPostList = true;

            if (postFromMap.post.poengVerdi !== postFromServer.poengVerdi ||
                    postFromMap.post.harRegistrert !== postFromServer.harRegistrert) {

                if (postFromMap.marker !== null) { map.removeLayer(postFromMap.marker); }
                console.log("post changed value or was taken. post id: " + idString + ", value: " + postFromServer.poengVerdi + ", oldValue: " + postFromMap.post.poengVerdi);
                postFromMap.marker = putPostOnMap(postFromServer);
                postFromMap.post = postFromServer;
            }
        });
        //Fjern poster fra kart om de ikke blir sendt fra serveren lengere
        postMarkersMap.forEach(function (postFromMap, key, mapObj) {
            if (postFromMap.inLastServerPostList === false) {
                if (postFromMap.marker !== null) { map.removeLayer(postFromMap.marker); }
                postMarkersMap.delete(key);
            } else {
                postFromMap.inLastServerPostList = false;
            }
        });
    }
}

function putPostOnMap(post) {
    var lat   = post.latitude;
    var lon   = post.longitude;
    var value = post.poengVerdi;
    var isRegistered = post.harRegistrert;

    var post_color = "gray";
    if (isRegistered=== false) {
        for (var i = 0; i < post_color_map.length; i++) {
            if (value > post_color_map[i][1]) {
                post_color = post_color_map[i][0];
                break;
            }
        }
    } else {
        return null;
    }

    var redMarker = L.AwesomeMarkers.icon({
        icon: 'flag',
        markerColor: post_color
    });

    var post_marker = L.marker([lat, lon], { icon: redMarker }).addTo(map); 

    post_marker.setZIndexOffset(-99);
    post_marker.bindPopup("" + value);

    return post_marker;
}

function clearPosts() {
    console.log("Remove all posts from map");
    postMarkersMap.forEach(function (postFromMap, key, mapObj) {
        if (postFromMap.marker !== null) { map.removeLayer(postFromMap.marker); }
    });
    postMarkersMap = new Map();
}

var players_and_markers = new Map();
var player_count = 0;
function updateTeamOnMap(players) {
    if (map !== null) {
        players.forEach(function (player) {
            if (player.navn === null) return;
            var player_and_marker = players_and_markers.get(player.navn);

            if (player_and_marker === undefined) { //new player
                console.log("Plasser ny spiller på brettet..." + player.navn);
                var marker = putPlayerOnMap(player, player_count,99999);
                player_and_marker = { 'player': player, 'marker': marker };
                players_and_markers.set(player.navn, player_and_marker);
                player_count++;

            } else { // old player
                var lat = (player.latitude);
                var lon = (player.longitude);
                var newLatLng = new L.LatLng(lat, lon);
                player_and_marker.marker.setLatLng(newLatLng);
            }
        });
    }
}

function putPlayerOnMap(player, color_index , zIndex) {
    var lat = player.latitude;
    var lon = player.longitude;
    var name = player.navn;

    var farge = post_color_map[color_index][0];

    var marker_icon = L.AwesomeMarkers.icon({
        icon: 'star',
        markerColor: 'blue',
        iconColor: farge
    });

    var player_marker = L.marker([lat, lon], { icon: marker_icon }).addTo(map);

    player_marker.setZIndexOffset(zIndex);
    player_marker.bindPopup("" + name);
    return player_marker;
}

function removeAllPlayersFromMap() {
    players_and_markers.forEach(function (item, key, mapObj) {
        map.removeLayer(item.marker);
    });
    players_and_markers = new Map();
}

var players_id_name_map = new Map();
function mapNames(players) {
    players.forEach(function (player) {
        var new_player = players_id_name_map.get(player.deltakerId);
        if (new_player === undefined) {
            console.log("new player: " + player.navn + ", deltakerId: " + player.deltakerId);
            players_id_name_map.set(player.deltakerId, player.navn);
        }
    });
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

        var name = players_id_name_map.get(msg.deltaker);
        if (name !== undefined) {
            msg.deltaker = name;
        } else {
            msg.deltaker = "Ukjent sender";
        }

        addNewMessage(msg);
    });

    if (rank_changed === true) {
        addNewMessage(rank_msg);
        rank_changed = false;
    }
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
    updatePostsOnMap(gameState.poster);
    updateScoreDiffToNextAndPrevTeam(gameState.ranking);
    mapNames(gameState.deltakere);

    var newTeam_score = gameState.score;
    if (team_score !== newTeam_score && team_score!==-1) {
        console.log("New score: " + newTeam_score);
        var diff = newTeam_score - team_score;
        if (diff > 0) {
            showToast("<b>+" + diff + "</b> poeng");
        } else {
            showToast("Felle! <b>" + diff + "</b> poeng ");
        }
    }
    team_score = newTeam_score;

    $(".team_status_score")[0].innerHTML = "<b> Score #" + team_score +"</b>";
    $(".team_status_score")[1].innerHTML = "<b> Score #" + team_score + "</b>";

    if(hasProcessedGameState===false){
        hasProcessedGameState = true;
        meldingsSekvens = 0;
        $("#messages_list")[0].innerHTML = ""; // remove all messages
    }
}

var n_bombs = 0;
var n_traps = 0;
function weaponsAviable(weapons) {
    n_bombs = 0;
    n_traps = 0;
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

function registerPostFromInput(input) {
    var postId = $("#post_id_" + input)[0].value;
    $("#post_id_" + input)[0].value = "";
    var weapon = "";
    if ($("#bomb_radio_" + input)[0].checked && n_bombs > 0) weapon = "BOMBE";
    if ($("#trap_radio_" + input)[0].checked && n_traps > 0) weapon = "FELLE";


    if (weapon === "" || $("#no_weapon_radio_" + input)[0].checked) {
        weapon = null;
    }

    console.log("Registrer post: " + postId + ", " + weapon);

    var successHandler = function () { showToast("Prøver å registrere post..."); getGameState(); };
    var errHandler = function () { showToast("Post ble ikke registrert. Sjekk at du er innlogget og har internett."); };

    registerPost(postId, weapon, successHandler, errHandler);
    autoSelectNoWeapon();
}

function registerPost(postId, weapon, successHandler, errHandler) {
    $.ajax({
        type: "POST",
        url: baseUrl + 'GameService',
        headers:
        createHeader(),
        data: JSON.stringify({
            'postKode': postId,
            'våpen': weapon
        }),
        success: successHandler,
        error: errHandler
    });
};

var player_position = {"lat":0, "lon":0};
function sendPosition_GPS() {
    navigator.geolocation.getCurrentPosition(success, null, { maximumAge: 0, timeout: 5000, enableHighAccuracy: true });
    function success(position) {
        player_position.lat = position.coords.latitude;
        player_position.lon = position.coords.longitude;
        var data = JSON.stringify({
            "latitude": player_position.lat,
            "longitude": player_position.lon
        });
        sendPosition(data);
    }
};

function sendPosition(data, successHandler, errHandler) {
    $.ajax({
        type: "POST",
        url: baseUrl + 'PosisjonsService',
        headers: createHeader(),
        data: data,
        success: successHandler,
        error: errHandler
    });
}

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
        
        logIn(lag_kode, deltaker_kode);
    } else {
        $('#options_modal').modal('show');
    }
}

function logIn(lag_kode, deltaker_kode) {
    console.log("prøver å logge inn... Lagkode: " + lag_kode + ", deltakerKode: " + deltaker_kode);

    meldingsSekvens = 0;
    $("#messages_list")[0].innerHTML = ""; // remove all messages
    removeAllPlayersFromMap(); // removes all prev team members
    clearPosts(); // removes all prev posts

    var successHandler = function () {
        showToast("Innlogget med lagkode: " + lag_kode + " og deltagerkode: " + deltaker_kode);
        console.log("Logget inn");
        loggedIn = true;
        hasProcessedGameState = false;
        mainLoop();
    };

    var errHandler = function () {
        showToast("Innlogging feilet");
        console.log("Innlogging feilet!");
        logOut();
    };

    var data = JSON.stringify({
        // thees are coordinates located in the middle of the pacific ocean.
        'latitude': 40.1111,
        'longitude': 170.1111
    });
    sendPosition(data, successHandler, errHandler);
}

function logOut() {
    $("#messages_list")[0].innerHTML = ""; // remove all messages
    removeAllPlayersFromMap(); // removes all prev team members
    clearPosts(); // removes all prev posts
    loggedIn = false;

    $(".team_status_score")[0].innerHTML = "Logg på";
    $(".team_status_score")[1].innerHTML = "Logg på";

    $(".team_status_rank")[0].innerHTML = "Logg på";
    $(".team_status_rank")[1].innerHTML = "Logg på";
}

// -------------------
// ---   Events    ---
// -------------------

//main loop
function mainLoop() {
    if (loggedIn === true) {
        getTeamPosition();
        sendPosition_GPS();
        getGameState();
        updateMessages();
    } else {
        console.log("Er ikke logget på");
    }
}

//Time-event
setInterval(function () {
    var auto_update = $("#radio_auto_update_yes")[0].checked;
    if (auto_update === true) {
        mainLoop();
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


    $("#rank_and_score_Carousel_main").carousel(); 
    $("#rank_and_score_Carousel_sx").carousel();

    $("#btn_switch_map_messages")[0].onclick = function () {
        switchMapAndMessages();
    }

    $("#send_messages_main")[0].onclick = function () {
        var msg = $("#send_messages_textbox_main")[0].value;
        $("#send_messages_textbox_main")[0].value = "";

        var successHandler = function () { showToast("Melding ble sendt"); };
        var errHandler = function () { showToast("Error"); };
        sendMessage(msg, successHandler, errHandler);
    }

    $("#send_melding_modal")[0].onclick = function () {
        var msg = $("#send_messages_textbox_modal")[0].value;
        $("#send_messages_textbox_modal")[0].value = "";
        
        var successHandler = function () { showToast("Melding ble sendt"); };
        var errHandler = function () { showToast("Error"); };
        sendMessage(msg, successHandler, errHandler);
    }

    $("#register_post_btn_main")[0].onclick = function () {
        registerPostFromInput("main");
    }
    $("#register_post_btn_modal")[0].onclick = function () {
        registerPostFromInput("modal");
    }

    
    $("#registrer_user")[0].onclick = function () {
        saveUserOption();
    }

    $("#bomb_btn_main")[0].onclick = function () {
        if ($("#bomb_btn_main")[0].classList.contains("disabled")) { setTimeout(autoSelectNoWeapon, 2); }
        else { autoSelectWeapon("BOMBE") }
    }
    $("#trap_btn_main")[0].onclick = function () {
        if ($("#trap_btn_main")[0].classList.contains("disabled")) { setTimeout(autoSelectNoWeapon, 2); }
        else { autoSelectWeapon("FELLE") }
    }
    $("#bomb_btn_modal")[0].onclick = function () {
        autoSelectWeapon("BOMBE");
    }
    $("#trap_btn_modal")[0].onclick = function () {
        autoSelectWeapon("FELLE");
    }
}

//Event that triggers when the screen changes size
$(window).resize(function () {
    updateAndDisplayMapOrMessage(map_isVisible);
}).resize()


// -------------------
// ---   Helpers   ---
// -------------------

var toastIsBeeingShown = false;
function showToast(msg, important) {
    var toast = document.getElementById("snackbar");
    var toast_msg = document.getElementById("toast_msg");

    
    if (toastIsBeeingShown === true && important === true) {

        // THIS IS NOT WORKING :(
        console.log("toast is important: " + msg);
        hideToast();
        setTimeout(showToast(msg,important), 100);
    }else if (toastIsBeeingShown === false) {
        toastIsBeeingShown = true
        console.log("toast: " + msg);

        toast_msg.innerHTML = msg;
        toast.classList.toggle("show", true);
        setTimeout(hideToast, 3000);
    } else {
        console.log("queued messages in toast: " + msg);
        setTimeout(function () {
            showToast(msg);
        }, 500);
    }
}

function hideToast() {
    var x = document.getElementById("snackbar");
    x.classList.toggle("show", false);
    toastIsBeeingShown = false;
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
        map_margin_right = 10;
        map_margin_left = 10;
        map_margin_top = 10;

        messages_margin_right = map_margin_right;
        messages_margin_left = map_margin_left;
        messages_margin_top = map_margin_top;

        map_margin_bottom = 30;
        messages_margin_bottom = 30;

        map_height = height_screen - map_margin_bottom - map_margin_top - 100;
        messages_height = map_height-30;
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
    $("#information_modal_body")[0].style.height = (height_screen - 175) + 'px';
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
    autoSelectWeapon(null);
}

function autoSelectWeapon(weapon){
    var input = ["modal", "main"];
    input.forEach(function (_i){
        $("#no_weapon_radio_" + _i).prop("checked", false);
        $("#bomb_radio_" + _i).prop("checked", false);
        $("#trap_radio_" + _i).prop("checked", false);

        $("#no_weapon_btn_" + _i)[0].classList.toggle("active", false);
        $("#bomb_btn_" + _i)[0].classList.toggle("active", false);
        $("#trap_btn_" + _i)[0].classList.toggle("active", false);

        if (weapon === "BOMBE") {
            $("#bomb_radio_" + _i).prop("checked", true);
            $("#bomb_btn_" + _i)[0].classList.toggle("active", true);
        } else if (weapon === "FELLE") {
            $("#trap_radio_" + _i).prop("checked", true);
            $("#trap_btn_" + _i)[0].classList.toggle("active", true);
        } else {
            $("#no_weapon_radio_" + _i).prop("checked", true);
            $("#no_weapon_btn_" + _i)[0].classList.toggle("active", true);
        }
    });
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