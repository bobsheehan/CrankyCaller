
var geoApp = angular.module('geoApp', ['ngAnimate', 'ngSanitize', 'ui.bootstrap']);



geoApp.controller('geoAppController', ['$scope', '$http', '$interval','$uibModal', function geoAppController($scope, $http, $interval,$uibModal) {

    $scope.initSetup = function (debug) {
        //general setup items
        $scope.trackingStatus = true;
        $scope.defaultZoom = 16; // street;
        $scope.maximumAge = 6000;
        $scope.message = "";
        $scope.whosLink = "";
        $scope.updateCount = 0;
        $scope.pinText = "You are here";
        $scope.displayAddress = false;
        $scope.displayDebugPanel = false;
        $scope.simulateWalk = false;
        $scope.phoneSearchMsg = "";
        $scope.googleGeoMsg = "";
        $scope.myClickListener = null;
        $scope.workingStatusIcon = "fa fa-refresh fa-spin";
        $scope.geocodeIconStatusWaiting="glyphicon glyphicon-eye-open";
        $scope.geocodeIconStatusWorking = $scope.workingStatusIcon;
        $scope.whoLivesHereStatusWaiting="glyphicon glyphicon-eye-open";
        $scope.whoLivesHereStatusWorking = $scope.workingStatusIcon;
        $scope.messageStatusIcon = "";
        
        $scope.whoLivesHereIconStatus = $scope.whoLivesHereStatusWaiting;
        $scope.geocodeIconStatus = $scope.geocodeIconStatusWaiting;
        
        $scope.clearResults();
    };

    $scope.initPhoneList = function () {
        $scope.phonesFound = [];
        $scope.showPhoneResults = true;
        $scope.phoneResult = 0;


    };

    $scope.initLocation = function () {
        //location tracking set up items
        $scope.watchId = -1;
        $scope.lat = 42.41696119;
        $scope.lon = -71.1710;
        $scope.lastLat = $scope.lat;
        $scope.lastLon = $scope.lon;

        //specific location variables
        $scope.addNum = "";
        $scope.addRoad = "";
        $scope.addSta = "";
        $scope.addZip = "";
        $scope.addTown = "";

    };

    $scope.clearResults = function () {
        $scope.spokeResults = [];
        $scope.spokeHomeVal = "";
        $scope.spokeResultsCount = 0;
        $scope.phonesFound = [];
        $scope.showPhoneResults = true;
        $scope.phoneResult = 0;
        $scope.message = "Results cleared...";
    };

    $scope.initJokes = function () {



        $scope.jokes = [{
            "id": "1",
            "type": 'Kid',
            "lines": [{
                "line1": "Is your refigerator running?",
                "line2": "Yes.",
                "line3": "We'll you better catch it before it runs out of the house."
            }]
        }, {
            id: "2",
            "type": 'Kid',
            "lines": [{
                "line1": "Is John Wall There?",
                "line2": "No.",
                "line3": "Are there any Walls there?",
                "line4": "No.",
                "line5": "Then what's holding up your house?"
            }]
        },
                        {
                            id: "3",
                            "type": 'Kid',
                            "lines": [{
                                "line1": "I'm calling about the broken pencils.",
                                "line2": "I'm sorry I don't understand what this is about.",
                                "line3": "Never mind... this is pointless."
                            }]
                        },
        {
            id: "4",
            "type": 'Kid',
            "lines": [{
                "line1": "I'm calling for Atcha.",
                "line2": "Acha who?",
                "line3": "Bless you."
            }]
        },
        {
            id: "5",
            "type": 'Kid',
            "lines": [{
                "line1": "I'm calling for IAM.",
                "line2": "IAM who?",
                "line3": "Well if you don't even know who you are then you obviously won't be able to find who I am calling for."
            }]
        }

        ];
    };
    
    $scope.initSetup(false);

    $scope.initLocation();

    $scope.initPhoneList();

    $scope.initJokes(); 
    
  $scope.amap= new google.maps.Map(document.getElementById('map'), {
          center: {lat: $scope.lat, lng: $scope.lon},
          zoom: $scope.defaultZoom
        });

   
  $scope.meMarker = new google.maps.Marker({
          position: {lat: $scope.lat, lng: $scope.lon},
          map: $scope.amap,
      
          title: $scope.pinText
        });

/*being google geocoder core functions */
    
  $scope.addGoogleGeoClickListner = function () {
      $scope.myClickListener = google.maps.event.addListener($scope.amap, 'click', function (event) {
          var clickLat = event.latLng.lat();
          var clickLon = event.latLng.lng();
          $scope.lat = clickLat;
          $scope.lon = clickLon;
          $scope.updateMap({
              lat: $scope.lat,
              lng: $scope.lon
          });

          $scope.geocodeLatLng();
      });
  };
    
  $scope.removeGoogleGeoListner = function () {
      if ($scope.myClickListener !== null) {
          $scope.amap.event.removeListener($scope.myClickListener);
      }
  };
    
 $scope.geocoder = new google.maps.Geocoder;
 
 $scope.makeYPlink = function () {
     // makes a quick link to Yellow page look up
     $scope.ypLink = "http://people.yellowpages.com/whitepages/address?street=" + $scope.addNum + "+" + $scope.addRoad + "&qloc=" + $scope.addTown + "+" + $scope.addState + "+" + $scope.addZip;

 };

 $scope.parseAddress = function (humanString) {
     //takes a human readable string and parses out data

     $scope.addNum = 0;
     $scope.addRoad = "";
     $scope.addSta = "";
     $scope.addZip = "";
     $scope.addTown = "";


     try {
         // omg - thank you https://github.com/hassansin/parse-address
         var parsed = parseAddress.parseLocation(humanString);
         $scope.addNum = parseInt(parsed.number);
         $scope.addRoad = $scope.if_undef(parsed.prefix, "").length > 0 ? parsed.prefix + ' ' + parsed.street : parsed.street;
         if (parsed.type.length > 0) { $scope.addRoad += ' ' + parsed.type; }
         $scope.addTown = parsed.city;
         $scope.addSta = parsed.state;
         $scope.addZip = parsed.zip;

     } catch (ex) {
         $scope.message = ex.message;
     }


 };
  
 $scope.geocodeLatLngSucess = function (results, status) {
     if (status === 'OK') {
         if (results[0]) {
             $scope.googleGeoMsg = results[0].formatted_address;
             $scope.parseAddress(results[0].formatted_address);
             $scope.displayAddress = true;
             $scope.makeYPlink();
             $scope.message = "Google says you are asking about this address...";
         } else {
             $scope.googleGeoMsg = 'No results found';
             $scope.message = "Google didn't find anthing...";
         }
         $scope.$apply();
     } else {
         $scope.googleGeoMsg = 'Geocoder failed due to: ' + status;
     }
 };
 
 $scope.geocodeLatLng = function () {

     $scope.clearResults();
     $scope.message = "Checking with Google.... be right back..";
     $scope.geocodeIconStatus= $scope.geocodeIconStatusWorking;
     var latlng = { lat: $scope.lat, lng: $scope.lon };

     $scope.geocoder.geocode({ 'location': latlng }, function (results, status) {
         $scope.geocodeLatLngSucess(results, status);
         $scope.geocodeIconStatus= $scope.geocodeIconStatusWaiting;
         $scope.$apply();
     });
 };
 
 /* end google geocoder core functions */  
  
  
  var updateTimer = function(){
      $scope.getLocation();
  };

  $scope.trackToggle = function(){
     if (!$scope.trackingStatus){
           navigator.geolocation.clearWatch($scope.watchId);
           $scope.removeGoogleGeoListner();
           $scope.addGoogleGeoClickListner();
         
     }else{
         
         $scope.getLocation();
        
     }
  };
      
  $scope.popFakePhone = function () {
      $scope.phoneResult++;
      var fakePhone = {
          "phonenumber": $scope.phoneResult + "23455",
          "name": "fakename" + $scope.phoneResult,
          "address": "fakeaddress goes here"
      };
      $scope.phonesFound.unshift(fakePhone);
      angular.forEach($scope.phonesFound, function (item, index) {
          console.log(item.name);
      });
  };

$scope.getDistanceFromLatLonInKm = function (lat1, lon1, lat2, lon2) {
      var R = 6371; // Radius of the earth in km
      var dLat = $scope.deg2rad(lat2 - lat1);  // deg2rad below
      var dLon = $scope.deg2rad(lon2 - lon1);
      var a =
        Math.sin(dLat / 2) * Math.sin(dLat / 2) +
        Math.cos($scope.deg2rad(lat1)) * Math.cos($scope.deg2rad(lat2)) *
        Math.sin(dLon / 2) * Math.sin(dLon / 2)
      ;
      var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
      var d = R * c; // Distance in km
      return d * 1000;
  };

$scope.deg2rad = function (deg) {
    return deg * (Math.PI / 180);
};
  
$scope.getSpokeTo = function () {

    // purpose: get spokeTo info

    $scope.clearResults();
    $scope.message = "Checking for names associated with this address...";
    $scope.whoLivesHereIconStatus = $scope.whoLivesHereStatusWorking;
   // $scope.$apply();
    $scope.showPhoneResults = false; // only show results when results come back
    
                var URL = "api/GeoSpokeTo?housenumber=" + $scope.addNum + "&road=" + $scope.addRoad + "&town=" + $scope.addTown + "&state=" + $scope.addSta + "&zip=" + $scope.addZip;
                //URL = "GeoSpoketo.json";
                $http.get(URL)
            .then(function (response) {

                $scope.spokeResults = response.data.peopleInfo;
                $scope.spokeHomeVal = response.data.houseValue;
                if (response.data.result < 1) {
                    $scope.updateGenMessage("No obvious names found here... try again later...");
                }else{
                 $scope.updateGenMessage("Found " +response.data.result + " name(s).");
                }
                $scope.whoLivesHereIconStatus = $scope.whoLivesHereStatusWaiting;

            }, function (error) {
                $scope.message = error.data;
                $scope.whoLivesHereIconStatus = $scope.whoLivesHereStatusWaiting;
            });



};

$scope.nameSearchSuccess = function (response, seq, fName, lName) {
    try {
        var responseData = response.data;
        if (responseData.result > 0) {

            angular.forEach(responseData.phoneList, function (phoneItem, index) {
                angular.forEach($scope.spokeResults, function (peopleItem, index) {
                    if (peopleItem.seq == seq) {
                        var nameParts = phoneItem.name.split(' ');
                        if (nameParts.length > 0) {
                            peopleItem.fName = nameParts[0];
                            peopleItem.lName = nameParts.slice(1).join(' ');
                        }
                        //peopleItem.fName = phoneItem.name;
                        peopleItem.phoneNumber = phoneItem.phoneNumber;
                    }
                });
            });
            $scope.message = "Deeper Search Got a hit.";
        } else {
            $scope.updateGenMessage("No results found for " + fName + " " + lName);
        }
    } catch (ex) {
        $scope.updateGenMessage(ex.message);
    }
};

$scope.nameSearch = function (fName, lName, seq) {
    $scope.message = "Deeper search for phone#s...";
    $scope.updateMessageIcon(1);
    try{
        $http.get("api/GeoYellowPageName?fName=" + fName + "&lName=" + lName + "&houseNumber=" + $scope.addNum + "&road=" + $scope.addRoad + "&town=" + $scope.addTown + "&state=" + $scope.addSta + "&zip=" + $scope.addZip)
    .then(function (response) {

           $scope.nameSearchSuccess(response, seq, fName, lName);
           $scope.updateMessageIcon(0);

    }, function (error) {
        $scope.message = error.message;
        $scope.updateMessageIcon(0);

    });
    } catch (ex) {
        $scope.updateGenMessage(ex.message);
        $scope.updateMessageIcon(0);

    }

};

$scope.getYBPhoneNums = function () {

    // purpose: get yellow book phone numbers

    $scope.phoneSearchIcon = "";
    $scope.phoneSearchMsgClass = "";
    $scope.message = "Checking for phone#s...";
    $scope.showPhoneResults = false; // only show results when results come back
    $http.get("api/GeoYellowPage?houseNumber=" + $scope.addNum + "&road=" + $scope.addRoad + "&town=" + $scope.addTown + "&state=" + $scope.addSta + "&zip=" + $scope.addZip)
.then(function (response) {
    // $scope.message = "Phone completed";
    //  $scope.phoneResult = response.data.result;
    $scope.phonepayload = response.data.phonelist;


    if (response.data.result > 0) {
        $scope.phoneSearchMsg = "Found " + response.data.result + " phone #s at this address";
        $scope.phoneSearchIcon = "glyphicon glyphicon-thumbs-up";
        $scope.phoneSearchMsgClass = "alert alert-success";
        angular.forEach($scope.phonepayload, function (item, index) {

            var phoneHit = {
                "phonenumber": item.phoneNumber,
                "name": item.name,
                "address": $scope.addNum + " " + $scope.addRoad
            };
            $scope.phonesFound.unshift(phoneHit);
            $scope.phoneResult++;
        });
    } else {
        $scope.phoneSearchIcon = "glyphicon glyphicon-thumbs-down";
        $scope.phoneSearchMsgClass = "alert alert-warning";

        $scope.phoneSearchMsg = "No phone #s found at this address.";
    }
    $scope.showPhoneResults = true;

  

}, function (error) {
    $scope.message = error.data;
});


};

$scope.updateMessageIcon = function (status) {
    /*0 =  done
     1 = working */
    if (status == 0) {
        $scope.messageStatusIcon = "";
    } else {
        $scope.messageStatusIcon = $scope.workingStatusIcon;

    }

};


$scope.updateGenMessage = function (msg) {
    $scope.message = msg;
};

$scope.getAddressFromGeo = function () {

      //  $http.get("openstreettest.json")
      $scope.clearResults();
      $scope.updateGenMessage ("calling address service");
      $scope.showPhoneResults = false;
      $http.get("api/Geowho?lat=" + $scope.lat + "&lon=" + $scope.lon)
.then(function (response) {
            $scope.updateGenMessage("Addr service completed");
            $scope.result = response.data.result;
            $scope.payload = JSON.parse(response.data.payload);

            try {
                $scope.addNum = $scope.if_undef($scope.payload.address.house_number, "");
                $scope.addRoad = $scope.if_undef($scope.payload.address.road, "");
                $scope.addTown = $scope.payload.address.town;
                $scope.addSta = $scope.payload.address.state;
                $scope.addZip = $scope.payload.address.postcode;

                //$scopewpLink = "http://people.yellowpages.com/whitepages/address?street=" + //$scope.addNum+"+" + $scope.addRoad + "&qloc=" + $scope.addTown +"+" +$scope.addState + //"+" +$scope.addZip;

            } catch (ex) {
                $scope.addNum = "";
                $scope.addRoad = "";
                $scope.addSta = "";
                $scope.addZip = "";
                $scope.addTown = "";
                $scope.message = ex.message;
            }
            //$scope.addNum = "70";

            if ($scope.addNum == "") {
                $scope.phoneSearchMsg = "Need a street # to look for phone numbers.";
            }
            $scope.makeYPlink();
            $scope.displayAddress = true;
        }, function (error) {
            $scope.message = error.data;
        });


  };

    $scope.isSearchableAddress = function () {
      return (
      ($scope.addNum !== "") &&
      ($scope.addRoad !== "") &&
      ($scope.addSta !== "") &&
      ($scope.addZip !== "") &&
      ($scope.addTown !== "")
      );
  };

$scope.if_undef = function (data, undefval) {
      if (undefined !== data) {
          return data;
      }
      else {
          return undefval;
      }


  };

  $scope.hasMoved = function () {
      return $scope.getDistanceFromLatLonInKm($scope.lat, $scope.lon, $scope.lastLat, $scope.lastLon);
  };

  $scope.displayPos = function (lat, lon) {
      // intended for human display
      return Math.round(lat * Math.pow(10, 5)) / Math.pow(10, 5) + " - " + parseFloat(Math.round(lon * 100) / 100).toFixed(5);
  };
  
  $scope.updateMap = function (pos) {
      //$scope.ainfoWindow.setPosition(pos);
      $scope.meMarker.setPosition(pos);
      //$scope.ainfoWindow.setContent($scope.pinText);
      $scope.amap.setCenter(pos);

      $scope.whosLink = "http://nominatim.openstreetmap.org/reverse?format=xml&zoom=18&addressdetails=1&lat=" + $scope.lat + "&lon=" + $scope.lon;

  };

  $scope.getLocation = function () {


      $scope.message = "Finding current location";

      $scope.lastLat = $scope.lat;
      $scope.lastLon = $scope.lon;
      if ($scope.simulateWalk) {
          var plusOrMinus = Math.random() < 0.1 ? -1 : 1;
          $scope.lat += Math.random(6) * .0902 * plusOrMinus;
          $scope.lon += Math.random(2) * .0002 * plusOrMinus;
          $scope.updateMap({
              lat: $scope.lat,
              lng: $scope.lon
          });

      } else {
          var geoOptions = {
              timeout: 10 * 1000,
              enableHighAccuracy: true,
              maximumAge: $scope.maximumAge

          };

          $scope.watchId = navigator.geolocation.watchPosition(function (position) {


              $scope.lastLat = $scope.lat;
              $scope.lastLon = $scope.lon;
              if ($scope.simulateWalk) {
                  var plusOrMinus = Math.random() < 0.5 ? -1 : 1;
                  $scope.lat += Math.random(6) * .0002 * plusOrMinus;
                  $scope.lon += Math.random(2) * .0002 * plusOrMinus;
              }
              else {
                  $scope.lat = position.coords.latitude;
                  $scope.lon = position.coords.longitude;
              }


              $scope.updateMap({ lat: $scope.lat, lng: $scope.lon });
              $scope.updateCount++;
              $scope.updateGenMessage("Map updated...");
              $scope.$apply();

          }, function () {
              $scope.updateGenMessage("geolocation failed");
              //handleLocationError(true, infoWindow, map.getCenter());
          }, geoOptions
   );
      }

  };
  
  $scope.getLocation();
 
}]);

geoApp.directive("debugPanel", function () {
    return {
        templateUrl: 'js/directives/debugpanel.html'

    };
});

geoApp.directive("newAddress", function () {
    return {
        templateUrl: 'js/directives/newaddress.html'
    };
});

geoApp.directive("searchResults", function () {
    return {
        templateUrl: 'js/directives/searchresults.html',
        scope: {
            lat: "@",
            phoneResultObj: "=",
            phonesFoundObj: "="
        }
    };
}

);
geoApp.directive("snoopResults", function () {
    return {
        templateUrl: 'js/directives/snoopresults.html'
    };
});

geoApp.directive('jokeDialog', function() {
    return {
        templateUrl: 'js/directives/jokedialog.html'
    };
});

