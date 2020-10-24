console.log("moving-map-content.js loaded");

if (chrome.runtime.onMessage) 
{
    chrome.runtime.onMessage.addListener(
        function(request, sender, sendResponse) {
            console.log("content-script onMessage");

            var resp = window[request.request]();

            resp.status = "200 OK";
            sendResponse(resp);
        });
}
var portNumberEx;
chrome.storage.sync.get({ portNumber: "8001" },
    function(data) {
        console.log("Value currently " + data.portNumber);
        portNumberEx = data.portNumber;

        var portDiv = document.createElement("div");
        portDiv.id = "portDivID";
        portDiv.style.display = "none";
        portDiv.innerText = portNumberEx;

        document.body.appendChild(portDiv);

        var mmScript = document.createElement("script");
        mmScript.textContent = 
        `var planeSize = 40;
    var lastLatLon = [];

    var portNumberDiv = document.getElementById("portDivID")
    //console.log("portNumberDiv "+portNumberDiv);
    var portNumber = portNumberDiv.innerText;

    function getHeading(lat1, lon1, lat2, lon2) 
    {
        var lat1 = lat1 * Math.PI / 180;
        var lat2 = lat2 * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;

        var y = Math.sin(dLon) * Math.cos(lat2);
        var x = Math.cos(lat1) * Math.sin(lat2) -
            Math.sin(lat1) * Math.cos(lat2) * Math.cos(dLon);

        var brng = Math.atan2(y, x);

        return (((brng * 180 / Math.PI) + 360) % 360);
    }

    function getLocation()
    {
    //console.log("getLocation");

        var xhr = new XMLHttpRequest();
        xhr.addEventListener("readystatechange", function () 
        {
            if (this.readyState === 4) 
            {
                //console.log("readyState: 4");
                if (this.status === 200) 
                {
                    //console.log("response: "+this.responseText);
                    var jsonResult = JSON.parse(this.responseText);
                    setMapIcon(jsonResult.coordinates);
                } 
                //else
                   // console.log("failed status: " + this.status);
            }
            //else
               // console.log("readyState: "+ this.readyState);
        });
        xhr.open("GET", "http://localhost:"+portNumber+"/get?position");
        xhr.send(null);
    };

    function setMapIcon(latlon) 
    {
        if (typeof SkyVector === 'undefined')
        {
            console.log("skyvector.com page not loaded");
            return;
        }

        var img = document.getElementById("movingAirplane");
        if (!img) 
        {
            img = document.createElement("img");
            img.id = "movingAirplane";
            img.src="data:image/png;base64,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";
            img.style.height = planeSize + "px";
            img.style.width = planeSize + "px";
            img.style.zIndex = "10";
            img.style.position = "absolute";
            var el = document.getElementById("sv_svgLayer");
            el.parentNode.insertBefore(img, el.nextSibling);
        }

        var point = SkyVector.ll2xy(latlon[0], latlon[1]);
        img.style.top = Math.round(point.y - SkyVector.data.slideroffsety - planeSize/2) + "px";
        img.style.left = Math.round(point.x - SkyVector.data.slideroffsetx - planeSize/2) + "px";
        var heading = getHeading(lastLatLon[0], lastLatLon[1], latlon[0], latlon[1]);
        img.style.transform = "rotate(" + heading + "deg)";
        img.style.visibility = "visible";

        lastLatLon = latlon;
    }

    setInterval(function() 
    { 
        getLocation()}, 2000);
        `;

        document.body.appendChild(mmScript);

    });

