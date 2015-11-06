angular.module('scoreboard', []);

angular.module('scoreboard').controller('scoreboardController', function($scope, $http, $interval) {
  $scope.hostname = "https://bbr2015.azurewebsites.net/api/";

  $scope.deltakerliste = [];
  $scope.lagliste = [];
  $scope.poster = [];
  $scope.kartposter = [];
  $scope.deltakerposisjoner = [];
  $scope.kartdeltakere = [];



  $scope.finnLagfarge = function(lagId) {
    for(var i = 1; i < $scope.lagliste.length; i++) {
      var lag = $scope.lagliste[i];
      if (lag.lagId === lagId) {
        return '#' + lag.lagFarge;
      }
    }
    return "#FFFFFF";
  };

  $scope.$watch('poster', function(newValue, oldValue) {
    var newPost = true;
    var oldKartpost;
    newValue.forEach(function(post) {
      for(var i = 0; i < $scope.kartposter.length; i++) {
        var kartpost = $scope.kartposter[i];
        if (kartpost.post.latitude === post.latitude && kartpost.post.longitude === post.longitude) {
          newPost = false;
          oldKartpost = kartpost;
          break;
        }
      }   
      if (newPost) {
          var awesomeMarker;
          if (post.riggetMedVåpen !== null) {
            awesomeMarker = L.AwesomeMarkers.icon({
                  icon: 'flag',
                  iconColor: 'red'
                });
          } else if (post.erSynlig) {
            awesomeMarker = L.AwesomeMarkers.icon({
                  icon: 'flag',
                  iconColor: 'blue'
                });
          } else {
            awesomeMarker = L.AwesomeMarkers.icon({
                  icon: 'flag',
                  iconColor: 'gray'
                });
          }
          var marker = L.marker([post.latitude, post.longitude], {icon: awesomeMarker, title: 'Post: ' + post.navn }).addTo(map);
          $scope.kartposter.push({post: post, marker: marker});
        } else {
          map.removeLayer(oldKartpost.marker);
          var awesomeMarker;
          console.log(oldKartpost.post);
          if (post.riggetMedVåpen) {
            awesomeMarker = L.AwesomeMarkers.icon({
                  icon: 'flag',
                  iconColor: 'red'
                });
          } else if (post.erSynlig) {
            awesomeMarker = L.AwesomeMarkers.icon({
                  icon: 'flag',
                  iconColor: 'blue'
                });
          } else {
            awesomeMarker = L.AwesomeMarkers.icon({
                  icon: 'flag',
                  iconColor: 'gray'
                });
          }
          console.log(awesomeMarker);
          var newMarker = L.marker([post.latitude, post.longitude], {icon: awesomeMarker, title: 'Post: ' + post.navn }).addTo(map);
          oldKartpost.marker = newMarker;
        }
    });
  });

  $scope.$watch('deltakerposisjoner', function(newValue, oldValue) {
    var newPlayer = true;
    var oldPlayer;
    newValue.forEach(function(team) {
      team.posisjoner.forEach(function(player) {
        for(var i = 0; i < $scope.kartdeltakere.length; i++) {
          var deltakerpos = $scope.kartdeltakere[i];
          if(deltakerpos.deltaker.deltakerId === player.deltakerId) {
            console.log(player);
            newPlayer = false;
            oldPlayer = deltakerpos;
            break;
          }
        }
        if (newPlayer) {
          var html = "";
          var lagFarge = $scope.finnLagfarge(player.lagId);
          if (player.lagId.indexOf('JAVA') > -1) {
            html = '<div style=\'background-color: ' + lagFarge +  ';height: 8px; width: 8px; border: 1px solid #101010; border-radius: 4px;\'></div>'; 
          } else {
            html = '<div style=\'background-color: ' + lagFarge +  ';height: 8px; width: 8px; border: 1px solid #101010\'></div>'; 
          }
          var myIcon = L.divIcon({className: 'java-marker', html: html});

          var marker = L.marker([player.latitude, player.longitude], 
                      {
                        icon: myIcon,
                        title: 'Spiller: ' + player.deltakerId + '\n Lag: ' + player.lagId
                      }).addTo(map);
          $scope.kartdeltakere.push({deltaker: player, marker: marker});
        } else {
          console.log("update pos");
          oldPlayer.marker.setLatLng([player.latitude, player.longitude]);
        }

      });
    });
  });

  
  $interval(function() {
    $http.get(
      $scope.hostname + "scoreboard",
        {headers: {"ScoreboardSecret": "en_liten_hemmelighet"}}
      ).then(
      function(response) {
        if (response.status === 200) {
          $scope.deltakerliste = response.data.deltakere;
          $scope.lagliste = response.data.lag;
          $scope.poster = response.data.poster;

          $http.get(
            $scope.hostname + "PosisjonsService/Alle",
            {headers: {"ScoreboardSecret": "en_liten_hemmelighet"}}
          ).then(
            function(response) {
              console.log(response);
              $scope.deltakerposisjoner = response.data;
            },
            function() {
              console.log("ERROR");
            }
          );
        }
      }, 
      function() {
      }
    );

    



  }, 3000);
  

  var map = L.map('map').setView([59.676598, 10.606179], 16);
  L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
  }).addTo(map);
});