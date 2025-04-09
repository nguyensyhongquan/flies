document.addEventListener('DOMContentLoaded', function() {
    // Xử lý thông báo lỗi từ server
    const errorMessage = '@ViewData["ErrorMessage"]';
    if (errorMessage) {
        Swal.fire({
            icon: 'error',
            title: 'Lỗi!',
            text: errorMessage,
            confirmButtonText: 'OK'
        });
    }

    // Xử lý form submit
    const form = document.querySelector('.form-valide');
    if (form) {
        form.addEventListener('submit', function(e) {
            e.preventDefault();
            
            // Lấy các giá trị từ form
            const username = document.getElementById('username').value;
            const fullname = document.getElementById('fullname').value;
            const email = document.getElementById('email').value;
            const mobilephone = document.getElementById('mobilephone').value;
            const password = document.getElementById('password').value;
            const confirmPassword = document.getElementById('confirmPassword').value;
            const gender = document.getElementById('gender').value;
            const balance = document.getElementById('balance').value;
            const birthday = document.getElementById('birthday').value;

            // Validate dữ liệu
            if (!username || !fullname || !email || !mobilephone || !password || !confirmPassword || !gender || !balance || !birthday) {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: 'Vui lòng điền đầy đủ thông tin!',
                    confirmButtonText: 'OK'
                });
                return;
            }

            if (password !== confirmPassword) {
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi!',
                    text: 'Mật khẩu xác nhận không khớp!',
                    confirmButtonText: 'OK'
                });
                return;
            }

            // Nếu validate thành công, submit form
            form.submit();
        });
    }
}); 