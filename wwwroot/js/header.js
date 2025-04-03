
// let isLoggedIn = false;
// let userName = "anhquanqh"; //
// let avatarUrl = "avatar.jpg"; //

// // Lấy các phần tử
// const authLinks = document.getElementById("authLinks");
// const userInfo = document.getElementById("userInfo");
// const loginLink = document.getElementById("loginLink");
// const signupLink = document.getElementById("signupLink");
// const logoutButton = document.getElementById("logoutButton");
// const userNameElement = document.getElementById("userName");
// const userAvatar = document.querySelector(".user-avatar");
// const googleLogin = document.getElementById("googleLogin");

// function updateUI() {
//     if (isLoggedIn) {
//         authLinks.style.display = "none";
//         userInfo.style.display = "flex";
//         googleLogin.style.display = "none";
//         userNameElement.textContent = userName;
//         userAvatar.src = avatarUrl;
//     } else {
//         authLinks.style.display = "flex";
//         userInfo.style.display = "none";
//         googleLogin.style.display = "flex";
//     }
// }

// logoutButton.addEventListener("click", () => {
//     isLoggedIn = false;
//     updateUI();
// });
document.querySelector(".user-avatar-container").addEventListener("click", function() {
    window.location.href = "profile.html";
});

window.onload = function() {
    updateUI();
};


document.getElementById('loginLink').addEventListener('click', function () {
    document.getElementById('authPopup').style.display = 'flex';
});

document.getElementById('signupLink').addEventListener('click', function () {
    document.getElementById('signupPopup').style.display = 'flex';
});

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
    button.addEventListener("click", function() {
        alert("Bạn đã nhấn nút Review!");
    });
});