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


