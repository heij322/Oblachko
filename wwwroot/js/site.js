document.querySelectorAll("img.photo-img").forEach(function (img) {
    function markMissing() {
        img.classList.add("is-missing");
    }
    if (!img.getAttribute("src") || (img.complete && img.naturalWidth === 0)) {
        markMissing();
    }
    img.addEventListener("error", markMissing);
    img.addEventListener("load", function () {
        img.classList.remove("is-missing");
    });
});

var menuPhoto = document.getElementById("menu-photo");
var menuCaption = document.getElementById("menu-caption");

document.querySelectorAll(".price-row").forEach(function (row) {
    function showProduct() {
        document.querySelectorAll(".price-row.active").forEach(function (other) {
            other.classList.remove("active");
        });
        row.classList.add("active");

        if (menuPhoto) {
            if (row.dataset.photo) {
                menuPhoto.src = row.dataset.photo;
            } else {
                menuPhoto.removeAttribute("src");
                menuPhoto.classList.add("is-missing");
            }
        }
        if (menuCaption) {
            menuCaption.textContent = row.dataset.caption;
        }
    }
    row.addEventListener("mouseenter", showProduct);
    row.addEventListener("focus", showProduct);
});

var pickButton = document.getElementById("pick-button");

if (pickButton) {
    var pickItems = Array.from(document.querySelectorAll("#pick-data li"));
    var pickName = document.getElementById("pick-name");
    var pickWhy = document.getElementById("pick-why");
    var pickPrice = document.getElementById("pick-price");
    var phrases = [
        "классика, не прогадаете",
        "день явно требует именно этого",
        "вы это заслужили",
        "как у бабушки",
        "берите два, второй на вечер",
        "отломите кусок по дороге",
        "кусочек счастья"
    ];
    var spinning = false;
    var lastIndex = -1;

    pickButton.addEventListener("click", function () {
        if (spinning || pickItems.length === 0) {
            return;
        }
        spinning = true;
        pickName.classList.remove("landed");
        pickWhy.textContent = "выбираем...";
        var steps = 0;

        var timer = setInterval(function () {
            var index;
            do {
                index = Math.floor(Math.random() * pickItems.length);
            } while (pickItems.length > 1 && index === lastIndex);
            lastIndex = index;

            var item = pickItems[index];
            pickName.textContent = item.dataset.name;
            pickName.href = item.dataset.url;
            pickPrice.textContent = item.dataset.price;

            steps++;
            if (steps >= 14) {
                clearInterval(timer);
                pickWhy.textContent = phrases[Math.floor(Math.random() * phrases.length)];
                pickName.classList.add("landed");
                spinning = false;
            }
        }, 80);
    });
}
