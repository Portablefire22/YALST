
function saveCurrentRegion() {
    let regionSelect = document.getElementById("region");
    let region = regionSelect.value;
    if (region === null || region === "") return;
    localStorage.setItem("region", region);
}

function loadRegion() {
    let region = localStorage.getItem("region");
    if (region === null || region === "") {
       saveCurrentRegion();
       return;
    }
    let regionSelect = document.getElementById("region");
    regionSelect.value = region;
}