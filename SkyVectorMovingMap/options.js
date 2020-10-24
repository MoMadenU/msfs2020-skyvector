console.log("Options loaded");
let portNumberElement = document.getElementById("portNumber");
let saveButtonElement = document.getElementById("saveButton");

saveButtonElement.addEventListener('click', function() {
    console.log("Save Button Click");
    if (portNumberElement.value === "")
        return;
    chrome.storage.sync.set({ portNumber: portNumberElement.value },
        function() {
            console.log("Saving port " + portNumberElement.value);
        });
});
try {
    chrome.storage.sync.get({ portNumber: "8001" },
        function(data) {
            console.log("Value currently " + data.portNumber);
            portNumberElement.value = data.portNumber;
        });
} catch (e) {
    console.log("failed to get port from storage");
}

//console.log("portNumberElement set to value: "+ portNumberElement.value);




