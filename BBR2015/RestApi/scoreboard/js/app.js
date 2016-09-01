angular.module('scoreboard', []);

angular.module('scoreboard').controller('scoreboardController', function ($scope, $http, $interval) {

    $scope.kartsentrumsatt = false;

    $scope.hostname = localStorage.serviceUrl; // updated in timer below

    $scope.deltakerliste = [];
    $scope.lagliste = [];
    $scope.poster = [];
    $scope.kartposter = [];
    $scope.deltakerposisjoner = [];
    $scope.kartdeltakere = [];
    $scope.eventmeldinger = [];

    $scope.kartsentrum = [];

    $scope.finnLagfarge = function (lagId) {
        for (var i = 0; i < $scope.lagliste.length; i++) {
            var lag = $scope.lagliste[i];
            if (lag.lagId === lagId) {
                return '#' + lag.lagFarge;
            }
        }
        return "#FFFFFF";
    };

    $scope.filtrerLagliste = function (filter) {
        return $scope.lagliste.filter(function (lag) { return lag.lagId.indexOf(filter) > -1; });
    };

    $scope.javapoeng = function () {
        var liste = $scope.filtrerLagliste("JAVA");
        var totalt = liste.reduce(function (a, b) { return a + b.score; }, 0);
        return parseInt(totalt / liste.length || 0);
    };

    $scope.microsoftpoeng = function () {
        var liste = $scope.filtrerLagliste("MS");
        var totalt = liste.reduce(function (a, b) { return a + b.score; }, 0);
        return parseInt(totalt / liste.length || 0);
    };



    $scope.$watch('poster', function (newValue, oldValue) {
        var newPost = true;
        var oldKartpost;
        newValue.forEach(function (post) {
            newPost = true;
            for (var i = 0; i < $scope.kartposter.length; i++) {
                var kartpost = $scope.kartposter[i];
                if (kartpost.post.latitude === post.latitude && kartpost.post.longitude === post.longitude) {
                    newPost = false;
                    oldKartpost = kartpost;
                    break;
                }
            }
            if (newPost) {
                var awesomeMarker;
                if (post.riggetMedVåpen) {
                    awesomeMarker = L.AwesomeMarkers.icon({
                        icon: 'flag',
                        markerColor: 'red'
                    });
                } else if (post.erSynlig) {
                    awesomeMarker = L.AwesomeMarkers.icon({
                        icon: 'flag',
                        markerColor: 'green'
                    });
                } else {
                    awesomeMarker = L.AwesomeMarkers.icon({
                        icon: 'flag',
                        markerColor: 'blue'
                    });
                }
                var marker = L.marker([post.latitude, post.longitude], { icon: awesomeMarker, title: 'Post: ' + post.navn }).addTo(map);
                $scope.kartposter.push({ post: post, marker: marker });
            } else {
                map.removeLayer(oldKartpost.marker);
                var awesomeMarker;
                if (post.riggetMedVåpen) {
                    awesomeMarker = L.AwesomeMarkers.icon({
                        icon: 'flag',
                        markerColor: 'red'
                    });
                } else if (post.erSynlig) {
                    awesomeMarker = L.AwesomeMarkers.icon({
                        icon: 'flag',
                        markerColor: 'green'
                    });
                } else {
                    awesomeMarker = L.AwesomeMarkers.icon({
                        icon: 'flag',
                        markerColor: 'blue'
                    });
                }
                var newMarker = L.marker([post.latitude, post.longitude], { icon: awesomeMarker, title: 'Post: ' + post.navn }).addTo(map);
                oldKartpost.marker = newMarker;
            }
        });
    });

    $scope.$watch('kartsentrum', function (newValue, oldValue) {
        if ($scope.kartsentrumsatt)
            return;

        map.setView(newValue, 16);

        $scope.kartsentrumsatt = true;
    });

    $scope.$watch('deltakerposisjoner', function (newValue, oldValue) {
        var newPlayer = true;
        var oldPlayer;
        newValue.forEach(function (team) {
            team.posisjoner.forEach(function (player) {
                for (var i = 0; i < $scope.kartdeltakere.length; i++) {
                    var deltakerpos = $scope.kartdeltakere[i];
                    if (deltakerpos.deltaker.deltakerId === player.deltakerId) {
                        newPlayer = false;
                        oldPlayer = deltakerpos;
                        break;
                    }
                }
                if (newPlayer) {
                    var html = "";
                    var lagFarge = $scope.finnLagfarge(player.lagId);
                    if (player.lagId.indexOf('JAVA') > -1) {
                        html = '<div style=\'background-color: ' + lagFarge + ';height: 10px; width: 10px; border: 1px solid #101010; border-radius: 5px;\'></div>';
                    } else {
                        html = '<div style=\'background-color: ' + lagFarge + ';height: 10px; width: 10px; border: 1px solid #101010\'></div>';
                    }
                    var myIcon = L.divIcon({ className: 'java-marker', html: html });
                    var marker = L.marker([player.latitude, player.longitude],
                                {
                                    icon: myIcon,
                                    title: 'Spiller: ' + player.deltakerId + '\n Lag: ' + player.lagId
                                }).addTo(map);
                    $scope.kartdeltakere.push({ deltaker: player, marker: marker });
                } else {
                    oldPlayer.marker.setLatLng([player.latitude, player.longitude]);
                }

            });
        });
    });

    $scope.sekvensid = "635824873779444105";

    $interval(function () {
        $scope.hostname = localStorage.serviceUrl;
        // , "MatchId": "4914b7b3-fa73-4340-b09c-2e1195859cf2"
        $http.get(
          $scope.hostname + "scoreboard",
            { headers: { "ScoreboardSecret": localStorage.scoreboardSecret } }
          ).then(
          function (response) {
              if (response.status === 200) {
                  $scope.deltakerliste = response.data.deltakere;
                  $scope.lagliste = response.data.lag;
                  $scope.poster = response.data.poster;                  
                  $scope.kartsentrum = response.data.match.centerMap;
                  $scope.matchNavn = response.data.match.navn;

                  $http.get(
                    $scope.hostname + "PosisjonsService/Alle",
                    { headers: { "ScoreboardSecret": localStorage.scoreboardSecret } }
                  ).then(
                    function (posResponse) {
                        $scope.deltakerposisjoner = posResponse.data;
                    },
                    function () {
                        console.log("ERROR");
                    }
                  );
              }
          },
          function () {
          });

        $http.get(
            $scope.hostname + "Meldinger/" + $scope.sekvensid,
            {
                headers: {
                    "Content-Type": "application/json",
                    "LagKode": localStorage.lagKode,
                    "DeltakerKode": localStorage.deltakerKode
                }
            }
            ).then(
              function (meldingsresponse) {
                  meldingsresponse.data.meldinger.forEach(function (melding) {
                      $scope.eventmeldinger.push(melding);
                  });
                  if (meldingsresponse.data.meldinger.length > 0) {
                      $scope.sekvensid = meldingsresponse.data.meldinger[0].sekvens;
                  }
              });
    }, 3000);


    var map = L.map('map').setView([59.9350517, 10.7564333], 16); 
    L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);

    $scope.downloadGoogleDrive = function() {
        var docId = localStorage.googleDriveDocId;

        if (docId && docId.length > 5) {
            $http.post($scope.hostname + "Admin/ConfigureFromGoogleDrive/" + docId,
            {},
                {
                    headers: { "ScoreboardSecret": localStorage.scoreboardSecret }
                }).then(function() {alert('Google Drive sync completed')});
        }
    };
});