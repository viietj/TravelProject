
var google;

function initMap() {
    // Check if Google Maps API is loaded
    if (typeof google === 'undefined' || typeof google.maps === 'undefined') {
        // Retry after a short delay
        setTimeout(initMap, 100);
        return;
    }

    // Basic options for a simple Google Map
    // For more options see: https://developers.google.com/maps/documentation/javascript/reference#MapOptions
    // var myLatlng = new google.maps.LatLng(40.71751, -73.990922);
    var myLatlng = new google.maps.LatLng(40.69847032728747, -73.9514422416687);
    // 39.399872
    // -8.224454

    var mapOptions = {
        // How zoomed in you want the map to start at (always required)
        zoom: 7,

        // The latitude and longitude to center the map (always required)
        center: myLatlng,

        // How you would like to style the map.
        scrollwheel: false,
        styles: [
            {
                "featureType": "administrative.country",
                "elementType": "geometry",
                "stylers": [
                    {
                        "visibility": "simplified"
                    },
                    {
                        "hue": "#ff0000"
                    }
                ]
            }
        ]
    };



    // Get the HTML DOM element that will contain your map
    // We are using a div with id="map" seen below in the <body>
    var mapElement = document.getElementById('map');

    if (!mapElement) {
        console.error('Map element not found');
        return;
    }

    // Create the Google Map using out element and options defined above
    var map = new google.maps.Map(mapElement, mapOptions);

    var addresses = ['New York'];

    for (var x = 0; x < addresses.length; x++) {
        $.getJSON('https://maps.googleapis.com/maps/api/geocode/json?address='+addresses[x]+'&key=AIzaSyBVWaKrjvy3MaE7SQ74_uJiULgl1JY0H2s', null, function (data) {
            if (data.status === 'OK' && data.results && data.results.length > 0) {
                var p = data.results[0].geometry.location;
                var latlng = new google.maps.LatLng(p.lat, p.lng);
                new google.maps.Marker({
                    position: latlng,
                    map: map,
                    icon: 'images/loc.png'
                });
            } else {
                console.warn('Geocoding failed for address:', addresses[x], data);
            }
        }).fail(function(jqXHR, textStatus, errorThrown) {
            console.error('Geocoding request failed:', textStatus, errorThrown);
        });
    }

}

// Initialize map when DOM is ready and Google Maps API is loaded
$(document).ready(function() {
    // Check if Google Maps is already loaded
    if (typeof google !== 'undefined' && typeof google.maps !== 'undefined') {
        initMap();
    } else {
        // Wait for Google Maps to load
        window.initMap = initMap;
    }
});