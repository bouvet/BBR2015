angular.module('scoreboard', []);

angular.module('scoreboard').controller('scoreboardController', function($scope, $http, $interval) {
  $scope.hostname = "https://bbr2015test.azurewebsites.net/api/";

  $scope.deltakerliste = [];
  $scope.lagliste = [];
  $scope.poster = [];
  $scope.kartposter = [];
  $scope.deltakerposisjoner = [];
  $scope.kartdeltakere = [];



  $scope.finnLagfarge = function(lagId) {
    for(var i = 1; i < $scope.lagliste.length; i++) {
      var lag = $scope.deltakerliste[i];
      if (lag.lagId === lagId) {
        return lag.lagFarge;
      }
    }
    return "#FFFFFF";
  }

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
          var marker = L.marker([post.latitude, post.longitude]).addTo(map);
          $scope.kartposter.push({post: post, marker: marker});
        } else {
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
            newPlayer = false;
            oldPlayer = deltakerpos;
            break;
          }
        }
        if (newPlayer) {
          var myIcon = L.divIcon({className: 'java-marker'});

          var marker = L.marker([player.latitude, player.longitude], 
                      {
                        icon: myIcon
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
        }
      }, 
      function() {

      }
    );

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



  }, 3000);
  

  var map = L.map('map').setView([59.676598, 10.606179], 16);
  L.tileLayer('http://{s}.tile.osm.org/{z}/{x}/{y}.png', {
    attribution: '&copy; <a href="http://osm.org/copyright">OpenStreetMap</a> contributors'
  }).addTo(map);

});