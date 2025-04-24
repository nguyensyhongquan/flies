
function openPopup(id) {
    if (id === 'editProfilePopup' && typeof currentUser !== 'undefined') {
        document.getElementById('editFullname').value = currentUser.fullName || "";
        document.getElementById('editBirthday').value = currentUser.birthday || "";
        document.getElementById('editPhone').value = currentUser.phone || "";
        document.getElementById('editAddress').value = currentUser.address || "";

        if (currentUser.gender === "M") {
            document.getElementById("genderMale").checked = true;
        } else if (currentUser.gender === "F") {
            document.getElementById("genderFemale").checked = true;
        } else {
            document.getElementById("genderOther").checked = true;
        }
    }
    document.getElementById(id).style.display = "flex";
}

function closePopup(id) {
    document.getElementById(id).style.display = "none";
}

function saveProfile() {
    const fullname = document.getElementById('editFullname').value;
    const birthday = document.getElementById('editBirthday').value;
    const gender = document.querySelector('input[name="gender"]:checked')?.value;
    const phone = document.getElementById('editPhone').value;
    const address = document.getElementById('editAddress').value;
     
    const formData = new URLSearchParams();
    formData.append("fullName", fullname);
    formData.append("birthday", birthday);
    formData.append("gender", gender);
    formData.append("phone", phone);
    formData.append("address", address);

    fetch('/Account/UpdateProfile', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                // Reload lại trang profile sau khi update thành công
                window.location.href = "/Account/ProfileStudent";
            } else {
                alert("Lỗi: " + data.message);
            }
        })
        .catch(err => {
            console.error("Lỗi khi gửi yêu cầu:", err);
            alert("Đã xảy ra lỗi.");
        });
}
// Function to open the change password popup
function openChangePasswordPopup() {
    document.getElementById('changePasswordPopup').style.display = 'flex';
}

// Function to close the change password popup
function closeChangePasswordPopup() {
    document.getElementById('changePasswordPopup').style.display = 'none';
}

// Function to save the new password
function savePassword() {
    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    if (newPassword !== confirmPassword) {
        alert("Mật khẩu mới và mật khẩu xác nhận không khớp.");
        return;
    }

    const formData = new URLSearchParams();
    formData.append("currentPassword", currentPassword);
    formData.append("newPassword", newPassword);
    formData.append("confirmPassword", confirmPassword);

    fetch('/Account/ChangePassword', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData
    })
        .then(res => {
            if (!res.ok) {
                throw new Error('Network response was not ok');
            }
            return res.json();
        })
        .then(data => {
            if (data.success) {
                alert("Đổi mật khẩu thành công!");
                closePopup('changePasswordPopup');
            } else {
                alert("Lỗi: " + data.message);
            }
        })
        .catch(err => {
            console.error("Lỗi khi gửi yêu cầu:", err);
            alert("Đã xảy ra lỗi.");
        });
}
// Mở popup Thay đổi Avatar
function openChangeAvatarPopup() {
    document.getElementById('avatarInput').value = '';
    document.getElementById('avatarPreviewContainer').style.display = 'none';
    document.getElementById('changeAvatarPopup').style.display = 'flex';
}

// Hàm preview ảnh khi chọn file
function previewAvatar(event) {
    const file = event.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = function (e) {
        const img = document.getElementById('avatarPreview');
        img.src = e.target.result;
        document.getElementById('avatarPreviewContainer').style.display = 'block';
    };
    reader.readAsDataURL(file);
}

// Hàm upload avatar lên server
function saveAvatar() {
    const input = document.getElementById('avatarInput');
    if (!input.files || input.files.length === 0) {
        alert('Vui lòng chọn ảnh.');
        return;
    }

    const file = input.files[0];
    const formData = new FormData();
    formData.append('avatar', file);

    fetch('/Account/ChangeAvatar', {
        method: 'POST',
        body: formData
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                // Cập nhật avatar trên trang
                document.querySelector('.profile-left .avatar').src = data.newAvatarUrl;
                alert('Cập nhật avatar thành công!');
                closePopup('changeAvatarPopup');
            } else {
                alert('Lỗi: ' + data.message);
            }
        })
        .catch(err => {
            console.error('Lỗi khi upload avatar:', err);
            alert('Đã xảy ra lỗi.');
        });
}
