
 logoutButton.addEventListener("click", () => {
     isLoggedIn = false;
     updateUI();
 });
document.querySelector(".user-avatar").addEventListener("click", function (event) {
    window.location.href = "/Account/ProfileStudent";
});

document.querySelector(".notification-icon").addEventListener("click", function (event) {
    window.location.href = "thongbao.html";
});

document.getElementById('loginLink').addEventListener('click', function () {
    document.getElementById('authPopup').style.display = 'flex';
});

document.getElementById('signupLink').addEventListener('click', function () {
    document.getElementById('signupPopup').style.display = 'flex';
});

function closePopup(popupId) {
    document.getElementById(popupId).style.display = 'none';
}

// Function to handle login
function login() {
    var username = document.getElementById('username').value;
    var password = document.getElementById('password').value;

    const userData = { username: username, passwordhash: password };
    console.log("🔹 Sending JSON:", JSON.stringify(userData));

    fetch('/Home/Login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(userData)
    })
        .then(response => response.text())
        .then(text => {
            console.log("🔹 Server Response:", text);
            try {
                const data = JSON.parse(text);
                if (data.success) {
                    document.getElementById('authLinks').style.display = 'none';
                    document.getElementById('googleLogin').style.display = 'none';
                    document.getElementById('userInfo').style.display = 'block';
                    document.getElementById('userName').textContent = data.username;

                    const userAvatar = data.avatar || '';
                    document.querySelector('.user-avatar').src = userAvatar;

                    window.location.href = data.homepageUrl;

                    closePopup('authPopup');
                } else {
                    alert(data.message);
                }
            } catch (err) {
                console.error("❌ JSON Parsing Error:", err, text);
            }
        })
        .catch(error => console.error('❌ Fetch error:', error));
}

document.getElementById('logoutButton').addEventListener('click', function () {
    fetch('/Account/Logout', { method: 'POST' })
        .then(response => {
            if (response.ok) {
                window.location.href = "/Account/Home";
            }
        });
});
fetch('/Home/CheckLoginStatus')
    .then(response => response.json())
    .then(data => {
        if (data.isLoggedIn) {
            document.getElementById('authLinks').style.display = 'none';
            document.getElementById('googleLogin').style.display = 'none';

            document.getElementById('userInfo').style.display = 'flex';
            document.getElementById('userName').textContent = data.username;

            const userAvatar = data.avatar && data.avatar !== "" ? data.avatar : '/images/user/avatar.jpg';
            document.querySelector('.user-avatar').src = userAvatar;
        } else {
            document.getElementById('authLinks').style.display = 'block';
            document.getElementById('googleLogin').style.display = 'block';

            document.getElementById('userInfo').style.display = 'none';
        }
    })
    .catch(error => console.error('❌ Fetch error:', error));
function closePopup(popupId) {
    document.getElementById(popupId).style.display = 'none';
}

document.querySelectorAll('.close-btn').forEach(button => {
    button.addEventListener('click', function () {
        this.closest('.popup').style.display = 'none';
    });
});

window.addEventListener('click', function (event) {
    document.querySelectorAll('.popup').forEach(popup => {
        if (event.target === popup) {
            popup.style.display = 'none';
        }
    });
});

function switchPopup(closeId, openId) {
    closePopup(closeId);
    document.getElementById(openId).style.display = 'flex';
}

document.getElementById("sendOtpBtn").addEventListener("click", function () {
    alert("OTP đã được gửi đến email của bạn!");
});
document.getElementById("signupBtn").addEventListener("click", function () {
    let password = document.getElementById("signupPassword").value;
    let confirmPassword = document.getElementById("confirmPassword").value;
    if (password !== confirmPassword) {
        alert("Mật khẩu nhập lại không khớp!");
    }
});
document.querySelectorAll(".review-btn").forEach(button => {
    button.addEventListener("click", function () {
        alert("Bạn đã nhấn nút Review!");
    });
});
