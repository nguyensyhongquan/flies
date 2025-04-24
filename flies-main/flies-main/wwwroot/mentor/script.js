// Main content loading functions
function loadHome() {
    document.getElementById("homePage").classList.remove("d-none");
    document.getElementById("dynamicContent").classList.add("d-none");
    loadCourses();
}

/**
 * Hiển thị thông báo tin nhắn trong giao diện
 * @param {string} message - Nội dung thông báo
 * @param {string} type - Loại thông báo ('success', 'danger', 'info')
 * @param {number} duration - Thời gian hiển thị (miliseconds), mặc định là 3000ms
 */
function showNotification(message, type = 'info', duration = 3000) {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} alert-dismissible fade show`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 1050;
        min-width: 250px;
    `;
    notification.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    document.body.appendChild(notification);

    // Tự động ẩn sau thời gian chỉ định
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => {
            notification.remove();
        }, 150); // Đợi hiệu ứng Bootstrap hoàn tất
    }, duration);
}

// Khôi phục danh sách tin nhắn giả lập
window.messages = [
    { id: 1, studentName: "Nguyễn Văn A", content: "Thầy ơi, em cần feedback cho bài Writing Task 1 ạ!", time: "09:20", isRead: false },
    { id: 2, studentName: "Vũ Văn H", content: "Em đang gặp khó với Listening, thầy có tài liệu nào không?", time: "09:22", isRead: false },
    { id: 3, studentName: "Hoàng Thị E", content: "Thầy giúp em cách viết conclusion Task 2 nhé!", time: "09:25", isRead: false }
];

// Biến để kiểm soát hiển thị badge chuông
let isNotificationHidden = false;

// Hàm cập nhật badge chuông thông báo
function updateNotificationBadge() {
    const notificationBell = document.querySelector('.btn.btn-light.position-relative');
    if (notificationBell) {
        const unreadCount = window.messages.filter(msg => !msg.isRead).length;
        notificationBell.innerHTML = `
            🔔
            ${!isNotificationHidden && unreadCount > 0 ? `
                <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill bg-danger">
                    ${unreadCount}
                    <span class="visually-hidden">unread messages</span>
                </span>
            ` : ''}
        `;
    }
}

// Hàm đánh dấu tin nhắn đã đọc
function markMessageAsRead(messageId) {
    const message = window.messages.find(msg => msg.id === messageId);
    if (message) {
        message.isRead = true;
        const messageCard = document.querySelector(`#message-${messageId} .status-btn`);
        if (messageCard) {
            messageCard.className = 'btn btn-sm btn-success status-btn';
            messageCard.textContent = 'Đã đọc';
        }
        updateNotificationBadge();
    }
}

// Hàm mở Chat Box với học sinh được chọn
function openChatWithStudent(studentName, messageId) {
    markMessageAsRead(messageId); // Đánh dấu tin nhắn là đã đọc
    showContent('chatBox', studentName); // Chuyển sang Chat Box với học sinh
}

// Sửa hàm showContent để hỗ trợ bất đồng bộ và truyền tham số cho getGradeTemplate
async function showContent(contentId, selectedStudent = null) {
    document.getElementById("homePage").classList.add("d-none");
    document.getElementById("dynamicContent").classList.remove("d-none");

    // Load appropriate content based on ID
    if (contentId === "myStudent") {
        loadClassroomSection();
    } else if (contentId === "chatBox") {
        // Gọi hàm từ message.js để load Chat Box, truyền selectedStudent
        if (typeof window.loadChatBox === "function") {
            window.loadChatBox(selectedStudent);
        } else {
            console.error("Hàm loadChatBox chưa được định nghĩa. Kiểm tra file message.js");
            document.getElementById("dynamicContent").innerHTML = "<h2>Lỗi</h2><p>Không thể tải Chat Box. Vui lòng kiểm tra lại.</p>";
        }
    } else {
        let content;
        if (contentId === "grade") {
            // Chờ getGradeTemplate hoàn tất và truyền tham số studentId, classroomId
            content = await window.getGradeTemplate("student123", "classroom456");
        } else {
            content = getContentTemplate(contentId);
        }
        console.log(`Đang render nội dung cho contentId: ${contentId}`);
        document.getElementById("dynamicContent").innerHTML = content;
        console.log("Nội dung dynamicContent sau khi render:", document.getElementById("dynamicContent").innerHTML);

        // Khởi tạo biểu đồ kỹ năng nếu đang hiển thị trang grade
        if (contentId === "grade") {
            setTimeout(() => {
                console.log("Đang gọi initGradeCharts...");
                const canvas = document.getElementById("skillsChart");
                if (canvas) {
                    console.log("Tìm thấy canvas với ID 'skillsChart'.");
                } else {
                    console.error("Không tìm thấy canvas với ID 'skillsChart' sau 100ms.");
                }
                if (typeof window.initGradeCharts === "function") {
                    window.initGradeCharts();
                } else {
                    console.error("Hàm initGradeCharts chưa được định nghĩa. Kiểm tra file grade.js");
                }
            }, 100);
        }

        // Thêm sự kiện cho nút trạng thái trong newMessage
        if (contentId === "newMessage") {
            document.querySelectorAll('.status-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.stopPropagation(); // Ngăn click nút trạng thái kích hoạt mở Chat Box
                    const messageId = parseInt(btn.dataset.messageId);
                    markMessageAsRead(messageId);
                });
            });
        }
    }
}

// Sửa hàm getContentTemplate để không gọi trực tiếp getGradeTemplate
function getContentTemplate(contentId) {
    const templates = {
        myCourse: "<h2>My Course</h2><p>Danh sách khóa học của bạn.</p>",
        addCourse: "<h2>Add Course</h2><p>Form thêm khóa học.</p>",
        addQuiz: "<h2>Add Quiz</h2><p>Form tạo bài kiểm tra.</p>",
        newMessage: `
            <div class="card p-4">
                <h2 class="mb-3">Tin Nhắn Mới</h2>
                <p class="text-muted mb-4">Danh sách tin nhắn từ học viên:</p>
                <div id="messageList">
                    ${generateMessageList()}
                </div>
            </div>
        `,
        chatBox: "", // Để trống vì dùng loadChatBox từ message.js
        grade: "", // Không gọi trực tiếp ở đây, sẽ xử lý trong showContent
    };

    return templates[contentId] || "<h2>Content Not Found</h2>";
}

// Hàm tạo danh sách thông báo
function generateMessageList() {
    return window.messages
        .map(
            (msg) => `
                <div class="card message-card mb-3" id="message-${msg.id}" onclick="openChatWithStudent('${msg.studentName}', ${msg.id})">
                    <div class="card-body">
                        <div class="d-flex align-items-center">
                            <img src="https://i.pravatar.cc/50?img=${msg.id}" alt="${msg.studentName}" class="rounded-circle">
                            <div class="flex-grow-1 ms-3">
                                <h6>${msg.studentName}</h6>
                                <p class="message-content mb-1">${msg.content}</p>
                                <small class="text-muted">${msg.time}</small>
                            </div>
                            <button class="btn btn-sm ${msg.isRead ? 'btn-success' : 'btn-warning'} status-btn" data-message-id="${msg.id}">
                                ${msg.isRead ? 'Đã đọc' : 'Chưa đọc'}
                            </button>
                        </div>
                    </div>
                </div>
            `
        )
        .join("");
}

// Classroom data
const classrooms = [
    {
        id: "writing1",
        title: "Ielts Writing Task 1",
        img: "asset/image/classzoom.jpeg",
    },
    {
        id: "writing2",
        title: "Ielts Writing Task 2",
        img: "asset/image/classzoom.jpeg",
    },
    { id: "reading", title: "Reading", img: "asset/image/classzoom.jpeg" },
    { id: "listening", title: "Listening", img: "asset/image/classzoom.jpeg" },
];

// Sample student data for each classroom with lessons
const studentsByClassroom = {
    writing1: [
        {
            id: 1,
            name: "Nguyễn Văn A",
            email: "nguyenvana@example.com",
            attendance: "85%",
            grade: "6.5",
            phone: "0912345678",
            address: "123 Nguyễn Huệ, Quận 1, TP.HCM",
            dateOfBirth: "15/06/2000",
            gender: "Nam",
            registrationDate: "05/01/2023",
            lastLogin: "22/04/2025",
            completedAssignments: 15,
            totalAssignments: 18,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Introduction to Bar Charts",
                    quizScore: 92,
                },
                {
                    type: "learning",
                    title: "Lesson 2: Describing Trends",
                    quizScore: 88,
                },
                {
                    type: "exam",
                    title: "Lesson 3: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Writing Task 1 - Test 1",
                        score: 82,
                        submittedAt: "2025-03-10T14:30:00Z",
                        content: "The bar chart shows the percentage of people using public transport in five different cities in 2020...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskAchievement: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 4: Final Test",
                    exam: {
                        examId: 2,
                        title: "Writing Task 1 - Test 2",
                        score: 75,
                        submittedAt: "2025-03-15T09:00:00Z",
                        content: "The line graph illustrates the number of tourists visiting a particular Caribbean island between 2010 and 2017...",
                        status: "graded",
                        remark: "Cần cải thiện phần từ vựng và cấu trúc câu. Bài viết khá mạch lạc nhưng cần thêm ví dụ cụ thể.",
                        criteria: {
                            taskAchievement: 7.5,
                            coherenceAndCohesion: 7.0,
                            lexicalResource: 6.5,
                            grammaticalRange: 7.0
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 5: Advanced Descriptions",
                    quizScore: 91,
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Test",
                    exam: {
                        examId: 3,
                        title: "Writing Task 1 - Test 3",
                        score: 88,
                        submittedAt: "2025-03-20T16:45:00Z",
                        content: "The pie chart compares the energy consumption of various sources in a country in 2019...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskAchievement: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 7: Final Assessment",
                    exam: {
                        examId: 4,
                        title: "Writing Task 1 - Test 4",
                        score: 91,
                        submittedAt: "2025-03-25T11:20:00Z",
                        content: "The table provides data on the average hours worked per week in different industries in 2022...",
                        status: "graded",
                        remark: "Bài viết rất tốt, sử dụng từ vựng phong phú và cấu trúc câu đa dạng. Cần chú ý lỗi nhỏ về chính tả.",
                        criteria: {
                            taskAchievement: 9.0,
                            coherenceAndCohesion: 8.5,
                            lexicalResource: 8.0,
                            grammaticalRange: 8.5
                        }
                    }
                }
            ],
            notes: "Học sinh chăm chỉ, có tinh thần học tập tốt. Cần cải thiện phần pronunciation.",
        },
        {
            id: 2,
            name: "Trần Thị B",
            email: "tranthib@example.com",
            attendance: "92%",
            grade: "A-",
            phone: "0923456789",
            address: "45 Lê Lợi, Quận 3, TP.HCM",
            dateOfBirth: "22/07/2001",
            gender: "Nữ",
            registrationDate: "10/01/2023",
            lastLogin: "23/04/2025",
            completedAssignments: 17,
            totalAssignments: 18,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Understanding Pie Charts",
                    quizScore: 95,
                },
                {
                    type: "exam",
                    title: "Lesson 2: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Writing Task 1 - Test 1",
                        score: 92,
                        submittedAt: "2025-03-10T15:00:00Z",
                        content: "The bar chart illustrates the proportion of students attending extra classes in four subjects...",
                        status: "graded",
                        remark: "Bài viết mạch lạc, sử dụng từ vựng tốt. Cần cải thiện phần diễn đạt ý tưởng phức tạp hơn.",
                        criteria: {
                            taskAchievement: 8.5,
                            coherenceAndCohesion: 8.0,
                            lexicalResource: 7.5,
                            grammaticalRange: 8.0
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 3: Progress Test",
                    exam: {
                        examId: 2,
                        title: "Writing Task 1 - Test 2",
                        score: 88,
                        submittedAt: "2025-03-15T10:30:00Z",
                        content: "The line graph shows the trend of urban population growth in three countries from 2000 to 2020...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskAchievement: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 4: Writing Comparisons",
                    quizScore: 90,
                },
                {
                    type: "exam",
                    title: "Lesson 5: Final Test",
                    exam: {
                        examId: 3,
                        title: "Writing Task 1 - Test 3",
                        score: 95,
                        submittedAt: "2025-03-20T14:00:00Z",
                        content: "The pie chart depicts the distribution of water usage in a typical household in 2021...",
                        status: "graded",
                        remark: "Bài viết xuất sắc, ý tưởng rõ ràng và từ vựng đa dạng. Giữ vững phong độ!",
                        criteria: {
                            taskAchievement: 9.5,
                            coherenceAndCohesion: 9.0,
                            lexicalResource: 8.5,
                            grammaticalRange: 9.0
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Assessment",
                    exam: {
                        examId: 4,
                        title: "Writing Task 1 - Test 4",
                        score: 89,
                        submittedAt: "2025-03-25T13:15:00Z",
                        content: "The table compares the sales figures of five different products in 2022...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskAchievement: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                }
            ],
            notes: "Học sinh có khả năng viết tốt, cần phát huy điểm mạnh này trong các bài thi.",
        },
        {
            id: 3,
            name: "Lê Minh C",
            email: "leminhc@example.com",
            attendance: "78%",
            grade: "B",
            phone: "0934567890",
            address: "78 Nguyễn Du, Quận 5, TP.HCM",
            dateOfBirth: "30/08/1999",
            gender: "Nam",
            registrationDate: "15/01/2023",
            lastLogin: "20/04/2025",
            completedAssignments: 13,
            totalAssignments: 18,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Basics of Line Graphs",
                    quizScore: 85,
                },
                {
                    type: "exam",
                    title: "Lesson 2: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Writing Task 1 - Test 1",
                        score: 76,
                        submittedAt: "2025-03-10T16:00:00Z",
                        content: "The bar chart shows the number of visitors to three museums in a city in 2020...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskAchievement: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 3: Progress Test",
                    exam: {
                        examId: 2,
                        title: "Writing Task 1 - Test 2",
                        score: 82,
                        submittedAt: "2025-03-15T11:00:00Z",
                        content: "The line graph illustrates the changes in electricity consumption in a country from 2015 to 2020...",
                        status: "graded",
                        remark: "Bài viết có ý tưởng tốt nhưng cần cải thiện phần ngữ pháp và từ vựng.",
                        criteria: {
                            taskAchievement: 7.0,
                            coherenceAndCohesion: 6.5,
                            lexicalResource: 6.0,
                            grammaticalRange: 6.5
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 4: Describing Pie Charts",
                    quizScore: 80,
                },
                {
                    type: "exam",
                    title: "Lesson 5: Final Test",
                    exam: {
                        examId: 3,
                        title: "Writing Task 1 - Test 3",
                        score: 79,
                        submittedAt: "2025-03-20T15:30:00Z",
                        content: "The pie chart shows the proportion of different types of vehicles in a city in 2021...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskAchievement: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Assessment",
                    exam: {
                        examId: 4,
                        title: "Writing Task 1 - Test 4",
                        score: 85,
                        submittedAt: "2025-03-25T12:00:00Z",
                        content: "The table provides information about the average monthly rainfall in three cities in 2022...",
                        status: "graded",
                        remark: "Bài viết khá tốt, nhưng cần chú ý cách diễn đạt để tránh lặp từ.",
                        criteria: {
                            taskAchievement: 8.0,
                            coherenceAndCohesion: 7.5,
                            lexicalResource: 7.0,
                            grammaticalRange: 7.5
                        }
                    }
                }
            ],
            notes: "Học sinh thường xuyên đi học muộn, cần nhắc nhở về việc tham dự đầy đủ.",
        },
    ],
    writing2: [
        {
            id: 4,
            name: "Phạm Văn D",
            email: "phamvand@example.com",
            attendance: "90%",
            grade: "A",
            phone: "0945678901",
            address: "234 Điện Biên Phủ, Quận Bình Thạnh, TP.HCM",
            dateOfBirth: "12/03/2000",
            gender: "Nam",
            registrationDate: "20/01/2023",
            lastLogin: "21/04/2025",
            completedAssignments: 16,
            totalAssignments: 17,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Opinion Essays",
                    quizScore: 90,
                },
                {
                    type: "exam",
                    title: "Lesson 2: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Writing Task 2 - Test 1",
                        score: 92,
                        submittedAt: "2025-03-12T09:00:00Z",
                        content: "Some people think that the best way to reduce crime is to give longer prison sentences. Others, however, believe there are better alternative ways of reducing crime. Discuss both views and give your opinion.",
                        status: "graded",
                        remark: "Bài viết rất tốt, lập luận chặt chẽ và từ vựng đa dạng. Cần chú ý một số lỗi ngữ pháp nhỏ.",
                        criteria: {
                            taskResponse: 9.0,
                            coherenceAndCohesion: 8.5,
                            lexicalResource: 8.0,
                            grammaticalRange: 8.5
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 3: Progress Test",
                    exam: {
                        examId: 2,
                        title: "Writing Task 2 - Test 2",
                        score: 95,
                        submittedAt: "2025-03-17T14:20:00Z",
                        content: "In some countries, the government spends a large amount of money on arts. Some people believe that this money should be spent on health and education instead. To what extent do you agree or disagree?",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskResponse: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 4: Argumentative Essays",
                    quizScore: 93,
                },
                {
                    type: "exam",
                    title: "Lesson 5: Final Test",
                    exam: {
                        examId: 3,
                        title: "Writing Task 2 - Test 3",
                        score: 90,
                        submittedAt: "2025-03-22T10:30:00Z",
                        content: "Many people believe that social networking sites have a huge negative impact on both individuals and society. To what extent do you agree?",
                        status: "graded",
                        remark: "Bài viết có lập luận rõ ràng, nhưng cần thêm ví dụ cụ thể để tăng tính thuyết phục.",
                        criteria: {
                            taskResponse: 8.5,
                            coherenceAndCohesion: 8.0,
                            lexicalResource: 7.5,
                            grammaticalRange: 8.0
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Assessment",
                    exam: {
                        examId: 4,
                        title: "Writing Task 2 - Test 4",
                        score: 94,
                        submittedAt: "2025-03-27T11:45:00Z",
                        content: "Some people think that the best way to increase road safety is to increase the minimum legal age for driving cars or motorbikes. To what extent do you agree or disagree?",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskResponse: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                }
            ],
            notes: "Học sinh có khả năng phân tích tốt, viết luận có chiều sâu.",
        },
        {
            id: 5,
            name: "Hoàng Thị E",
            email: "hoangthie@example.com",
            attendance: "95%",
            grade: "A+",
            phone: "0956789012",
            address: "56 Trương Định, Quận 10, TP.HCM",
            dateOfBirth: "25/05/2001",
            gender: "Nữ",
            registrationDate: "25/01/2023",
            lastLogin: "23/04/2025",
            completedAssignments: 17,
            totalAssignments: 17,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Essay Structure",
                    quizScore: 98,
                },
                {
                    type: "exam",
                    title: "Lesson 2: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Writing Task 2 - Test 1",
                        score: 98,
                        submittedAt: "2025-03-12T10:00:00Z",
                        content: "Some people believe that children should be allowed to make their own decisions on everyday matters. Others believe that children should follow their parents’ decisions. Discuss both views and give your own opinion.",
                        status: "graded",
                        remark: "Bài viết xuất sắc, lập luận rất thuyết phục và sử dụng từ vựng phong phú. Giữ vững phong độ!",
                        criteria: {
                            taskResponse: 9.5,
                            coherenceAndCohesion: 9.0,
                            lexicalResource: 9.0,
                            grammaticalRange: 9.0
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 3: Progress Test",
                    exam: {
                        examId: 2,
                        title: "Writing Task 2 - Test 2",
                        score: 96,
                        submittedAt: "2025-03-17T15:00:00Z",
                        content: "In many countries, the gap between the rich and the poor is increasing. What problems does this cause, and what solutions can you suggest?",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskResponse: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 4: Advanced Essay Writing",
                    quizScore: 97,
                },
                {
                    type: "exam",
                    title: "Lesson 5: Final Test",
                    exam: {
                        examId: 3,
                        title: "Writing Task 2 - Test 3",
                        score: 97,
                        submittedAt: "2025-03-22T11:00:00Z",
                        content: "Some people think that technology has made our lives more complex, while others believe it has made our lives easier. Discuss both views and give your opinion.",
                        status: "graded",
                        remark: "Bài viết rất ấn tượng, lập luận chặt chẽ và sử dụng ngôn ngữ tự nhiên. Cần chú ý một số lỗi nhỏ về dấu câu.",
                        criteria: {
                            taskResponse: 9.0,
                            coherenceAndCohesion: 9.0,
                            lexicalResource: 8.5,
                            grammaticalRange: 9.0
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Assessment",
                    exam: {
                        examId: 4,
                        title: "Writing Task 2 - Test 4",
                        score: 99,
                        submittedAt: "2025-03-27T12:30:00Z",
                        content: "Some people believe that the government should provide free healthcare for all citizens. To what extent do you agree or disagree?",
                        status: "pending",
                        remark: "",
                        criteria: {
                            taskResponse: null,
                            coherenceAndCohesion: null,
                            lexicalResource: null,
                            grammaticalRange: null
                        }
                    }
                }
            ],
            notes: "Học sinh xuất sắc, luôn hoàn thành bài tập đúng hạn với chất lượng cao.",
        },
    ],
    reading: [
        {
            id: 6,
            name: "Đỗ Văn F",
            email: "dovanf@example.com",
            attendance: "82%",
            grade: "B",
            phone: "0967890123",
            address: "89 Cách Mạng Tháng 8, Quận 3, TP.HCM",
            dateOfBirth: "18/11/2000",
            gender: "Nam",
            registrationDate: "02/02/2023",
            lastLogin: "19/04/2025",
            completedAssignments: 12,
            totalAssignments: 15,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Skimming Techniques",
                    quizScore: 78,
                },
                {
                    type: "exam",
                    title: "Lesson 2: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Reading Test 1",
                        score: 78,
                        submittedAt: "2025-03-11T13:00:00Z",
                        content: "Answers to Passage 1: True, False, Not Given...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            accuracy: null,
                            speed: null,
                            comprehension: null,
                            vocabularyUsage: null
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 3: Progress Test",
                    exam: {
                        examId: 2,
                        title: "Reading Test 2",
                        score: 85,
                        submittedAt: "2025-03-16T14:00:00Z",
                        content: "Answers to Passage 2: Multiple Choice, Matching Headings...",
                        status: "graded",
                        remark: "Cần cải thiện tốc độ đọc và độ chính xác trong phần trả lời câu hỏi True/False.",
                        criteria: {
                            accuracy: 7.5,
                            speed: 7.0,
                            comprehension: 7.0,
                            vocabularyUsage: 7.5
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 4: Scanning Techniques",
                    quizScore: 82,
                },
                {
                    type: "exam",
                    title: "Lesson 5: Final Test",
                    exam: {
                        examId: 3,
                        title: "Reading Test 3",
                        score: 80,
                        submittedAt: "2025-03-21T15:00:00Z",
                        content: "Answers to Passage 3: Sentence Completion, Summary Completion...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            accuracy: null,
                            speed: null,
                            comprehension: null,
                            vocabularyUsage: null
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Assessment",
                    exam: {
                        examId: 4,
                        title: "Reading Test 4",
                        score: 83,
                        submittedAt: "2025-03-26T16:00:00Z",
                        content: "Answers to Passage 4: Matching Information, Yes/No/Not Given...",
                        status: "graded",
                        remark: "Học sinh có tiến bộ trong việc tìm thông tin, nhưng cần chú ý phần từ vựng.",
                        criteria: {
                            accuracy: 7.5,
                            speed: 7.5,
                            comprehension: 7.0,
                            vocabularyUsage: 7.0
                        }
                    }
                }
            ],
            notes: "Học sinh cần cải thiện kỹ năng đọc hiểu và tốc độ đọc.",
        },
        {
            id: 7,
            name: "Ngô Thị G",
            email: "ngothig@example.com",
            attendance: "88%",
            grade: "B+",
            phone: "0978901234",
            address: "123 Hai Bà Trưng, Quận 1, TP.HCM",
            dateOfBirth: "07/04/2001",
            gender: "Nữ",
            registrationDate: "05/02/2023",
            lastLogin: "22/04/2025",
            completedAssignments: 14,
            totalAssignments: 15,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Vocabulary Building",
                    quizScore: 85,
                },
                {
                    type: "exam",
                    title: "Lesson 2: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Reading Test 1",
                        score: 87,
                        submittedAt: "2025-03-11T14:00:00Z",
                        content: "Answers to Passage 1: True, False, Not Given...",
                        status: "graded",
                        remark: "Học sinh trả lời chính xác phần lớn câu hỏi, cần cải thiện tốc độ đọc.",
                        criteria: {
                            accuracy: 8.0,
                            speed: 7.5,
                            comprehension: 8.0,
                            vocabularyUsage: 7.5
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 3: Progress Test",
                    exam: {
                        examId: 2,
                        title: "Reading Test 2",
                        score: 84,
                        submittedAt: "2025-03-16T15:00:00Z",
                        content: "Answers to Passage 2: Multiple Choice, Matching Headings...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            accuracy: null,
                            speed: null,
                            comprehension: null,
                            vocabularyUsage: null
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 4: Reading Strategies",
                    quizScore: 88,
                },
                {
                    type: "exam",
                    title: "Lesson 5: Final Test",
                    exam: {
                        examId: 3,
                        title: "Reading Test 3",
                        score: 88,
                        submittedAt: "2025-03-21T16:00:00Z",
                        content: "Answers to Passage 3: Sentence Completion, Summary Completion...",
                        status: "graded",
                        remark: "Học sinh làm tốt phần matching headings, cần chú ý phần từ vựng.",
                        criteria: {
                            accuracy: 8.5,
                            speed: 8.0,
                            comprehension: 8.0,
                            vocabularyUsage: 7.5
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Assessment",
                    exam: {
                        examId: 4,
                        title: "Reading Test 4",
                        score: 89,
                        submittedAt: "2025-03-26T17:00:00Z",
                        content: "Answers to Passage 4: Matching Information, Yes/No/Not Given...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            accuracy: null,
                            speed: null,
                            comprehension: null,
                            vocabularyUsage: null
                        }
                    }
                }
            ],
            notes: "Học sinh có khả năng tìm kiếm thông tin nhanh, cần phát triển kỹ năng đọc suy luận.",
        },
    ],
    listening: [
        {
            id: 8,
            name: "Vũ Văn H",
            email: "vuvanh@example.com",
            attendance: "75%",
            grade: "C+",
            phone: "0989012345",
            address: "46 Lý Thường Kiệt, Quận Tân Bình, TP.HCM",
            dateOfBirth: "29/09/1999",
            gender: "Nam",
            registrationDate: "10/02/2023",
            lastLogin: "15/04/2025",
            completedAssignments: 10,
            totalAssignments: 16,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Listening for Details",
                    quizScore: 70,
                },
                {
                    type: "exam",
                    title: "Lesson 2: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Listening Test 1",
                        score: 72,
                        submittedAt: "2025-03-13T09:00:00Z",
                        content: "Answers: 1. John Smith, 2. 25th April, 3. 123-456-7890...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            comprehension: null,
                            recognition: null,
                            accuracy: null,
                            attentionToDetail: null
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 3: Progress Test",
                    exam: {
                        examId: 2,
                        title: "Listening Test 2",
                        score: 68,
                        submittedAt: "2025-03-18T10:00:00Z",
                        content: "Answers: 1. Library, 2. 10:30 AM, 3. Book Club...",
                        status: "graded",
                        remark: "Cần cải thiện khả năng nhận diện từ khóa và tập trung vào chi tiết.",
                        criteria: {
                            comprehension: 6.5,
                            recognition: 6.0,
                            accuracy: 6.0,
                            attentionToDetail: 6.0
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 4: Listening for Main Ideas",
                    quizScore: 72,
                },
                {
                    type: "exam",
                    title: "Lesson 5: Final Test",
                    exam: {
                        examId: 3,
                        title: "Listening Test 3",
                        score: 75,
                        submittedAt: "2025-03-23T11:00:00Z",
                        content: "Answers: 1. Sydney, 2. 3rd May, 3. 987-654-3210...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            comprehension: null,
                            recognition: null,
                            accuracy: null,
                            attentionToDetail: null
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Assessment",
                    exam: {
                        examId: 4,
                        title: "Listening Test 4",
                        score: 78,
                        submittedAt: "2025-03-28T12:00:00Z",
                        content: "Answers: 1. Museum, 2. 2:00 PM, 3. History Tour...",
                        status: "graded",
                        remark: "Học sinh có tiến bộ, nhưng cần luyện nghe với accent khác nhau.",
                        criteria: {
                            comprehension: 7.0,
                            recognition: 6.5,
                            accuracy: 6.5,
                            attentionToDetail: 7.0
                        }
                    }
                }
            ],
            notes: "Học sinh gặp khó khăn với accent của người bản xứ, cần thêm thời gian luyện tập.",
        },
        {
            id: 9,
            name: "Mai Thị I",
            email: "maithii@example.com",
            attendance: "93%",
            grade: "A-",
            phone: "0990123456",
            address: "78 Nguyễn Thị Minh Khai, Quận 3, TP.HCM",
            dateOfBirth: "14/02/2000",
            gender: "Nữ",
            registrationDate: "15/02/2023",
            lastLogin: "23/04/2025",
            completedAssignments: 15,
            totalAssignments: 16,
            lessons: [
                {
                    type: "learning",
                    title: "Lesson 1: Listening for Numbers",
                    quizScore: 88,
                },
                {
                    type: "exam",
                    title: "Lesson 2: Mid-term Test",
                    exam: {
                        examId: 1,
                        title: "Listening Test 1",
                        score: 90,
                        submittedAt: "2025-03-13T10:00:00Z",
                        content: "Answers: 1. Anna Brown, 2. 15th June, 3. 456-789-1234...",
                        status: "graded",
                        remark: "Học sinh làm rất tốt, nhận diện từ khóa chính xác.",
                        criteria: {
                            comprehension: 8.5,
                            recognition: 8.0,
                            accuracy: 8.0,
                            attentionToDetail: 8.5
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 3: Progress Test",
                    exam: {
                        examId: 2,
                        title: "Listening Test 2",
                        score: 93,
                        submittedAt: "2025-03-18T11:00:00Z",
                        content: "Answers: 1. Park, 2. 9:00 AM, 3. Yoga Class...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            comprehension: null,
                            recognition: null,
                            accuracy: null,
                            attentionToDetail: null
                        }
                    }
                },
                {
                    type: "learning",
                    title: "Lesson 4: Listening for Specific Information",
                    quizScore: 90,
                },
                {
                    type: "exam",
                    title: "Lesson 5: Final Test",
                    exam: {
                        examId: 3,
                        title: "Listening Test 3",
                        score: 88,
                        submittedAt: "2025-03-23T12:00:00Z",
                        content: "Answers: 1. London, 2. 10th July, 3. 321-654-9876...",
                        status: "graded",
                        remark: "Học sinh làm tốt, nhưng cần chú ý phần chi tiết nhỏ như số điện thoại.",
                        criteria: {
                            comprehension: 8.0,
                            recognition: 8.0,
                            accuracy: 7.5,
                            attentionToDetail: 8.0
                        }
                    }
                },
                {
                    type: "exam",
                    title: "Lesson 6: Extra Assessment",
                    exam: {
                        examId: 4,
                        title: "Listening Test 4",
                        score: 91,
                        submittedAt: "2025-03-28T13:00:00Z",
                        content: "Answers: 1. Cinema, 2. 7:00 PM, 3. Movie Night...",
                        status: "pending",
                        remark: "",
                        criteria: {
                            comprehension: null,
                            recognition: null,
                            accuracy: null,
                            attentionToDetail: null
                        }
                    }
                }
            ],
            notes: "Học sinh có khả năng nghe tốt, đặc biệt là phân biệt các accent khác nhau.",
        },
    ],
};

// Hàm lưu điểm và nhận xét
function saveRemark(studentId, classroomId, examId) {
    const student = studentsByClassroom[classroomId].find(s => s.id === studentId);
    const lesson = student.lessons.find(l => l.exam && l.exam.examId === examId);
    const exam = lesson ? lesson.exam : null;

    if (!exam) return;

    // Lấy giá trị từ các input
    const taskAchievement = parseFloat(document.getElementById(`taskAchievement-${examId}`).value);
    const coherenceAndCohesion = parseFloat(document.getElementById(`coherenceAndCohesion-${examId}`).value);
    const lexicalResource = parseFloat(document.getElementById(`lexicalResource-${examId}`).value);
    const grammaticalRange = parseFloat(document.getElementById(`grammaticalRange-${examId}`).value);
    const remark = document.getElementById(`remark-${examId}`).value;

    // Tính điểm trung bình
    const newScore = (taskAchievement + coherenceAndCohesion + lexicalResource + grammaticalRange) / 4;

    // Cập nhật dữ liệu
    exam.criteria.taskAchievement = exam.criteria.taskAchievement !== undefined ? taskAchievement : null;
    exam.criteria.taskResponse = exam.criteria.taskResponse !== undefined ? taskAchievement : null;
    exam.criteria.coherenceAndCohesion = coherenceAndCohesion;
    exam.criteria.lexicalResource = lexicalResource;
    exam.criteria.grammaticalRange = grammaticalRange;
    exam.score = Math.round(newScore * 10); // Quy đổi điểm sang thang 100
    exam.remark = remark;
    exam.status = "graded";

    // Cập nhật giao diện
    showStudentDetails(studentId, classroomId);

    // Đóng modal
    const modal = bootstrap.Modal.getInstance(document.getElementById(`remarkModal-${examId}`));
    modal.hide();

    // Hiển thị thông báo
    showNotification("Đã lưu nhận xét và điểm số!", "success", 3000);
}

// Show student cards for a specific classroom
function showClassroomStudents(classroomId) {
    document.getElementById("classroomListContainer").classList.add("d-none");
    document.getElementById("studentTableContainer").classList.remove("d-none");

    const classroom = classrooms.find((c) => c.id === classroomId);
    const students = studentsByClassroom[classroomId];

    document.getElementById(
        "selectedClassroomTitle"
    ).textContent = `${classroom.title} - Danh sách học viên`;

    let cardsHtml = "";
    students.forEach((student) => {
        const avatarUrl = `https://i.pravatar.cc/100?img=${student.id + 10}`;
        cardsHtml += `
            <div class="col-md-3 mb-4">
                <div class="card student-card text-center p-3" onclick="showStudentDetails(${student.id}, '${classroomId}')">
                    <div class="text-center mb-3">
                        <img src="${avatarUrl}" alt="${student.name}" class="img-fluid" style="max-width: 80px; border-radius: 50%;">
                    </div>
                    <h5>${student.name}</h5>
                    <p class="text-muted small">${student.email}</p>
                    <div class="d-flex justify-content-between mt-2">
                        <span class="badge bg-info">Tham gia: ${student.attendance}</span>
                        <span class="badge bg-success">Điểm: ${student.grade}</span>
                    </div>
                </div>
            </div>
            `;
    });

    document.getElementById("studentCardContainer").innerHTML = cardsHtml;

    document.querySelectorAll(".student-card").forEach((card) => {
        card.style.transition = "transform 0.3s ease, box-shadow 0.3s ease";
        card.style.cursor = "pointer";

        card.addEventListener("mouseenter", () => {
            card.style.transform = "translateY(-5px)";
            card.style.boxShadow = "0 10px 20px rgba(0,0,0,0.1)";
        });

        card.addEventListener("mouseleave", () => {
            card.style.transform = "translateY(0)";
            card.style.boxShadow = "none";
        });
    });
}

// Go back to classroom list
function backToClassrooms() {
    document.getElementById("classroomListContainer").classList.remove("d-none");
    document.getElementById("studentTableContainer").classList.add("d-none");
    document.getElementById("studentDetailContainer")?.classList.add("d-none");
}

// Hàm hiển thị modal chấm bài
function showRemarkModal(studentId, classroomId, examId) {
    const student = studentsByClassroom[classroomId].find(s => s.id === studentId);
    const lesson = student.lessons.find(l => l.exam && l.exam.examId === examId);
    const exam = lesson ? lesson.exam : null;

    if (!exam) return;

    const modalHTML = `
        <div class="modal fade" id="remarkModal-${examId}" tabindex="-1" aria-labelledby="remarkModalLabel" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title" id="remarkModalLabel">Chấm bài: ${exam.title}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body">
                        <p><strong>Học sinh:</strong> ${student.name}</p>
                        <p><strong>Ngày nộp:</strong> ${new Date(exam.submittedAt).toLocaleString('vi-VN')}</p>
                        <p><strong>Nội dung bài làm:</strong></p>
                        <p class="border p-3 bg-light">${exam.content}</p>
                        <h6>Điểm chi tiết:</h6>
                        <div class="mb-3">
                            <label class="form-label">Task Achievement / Task Response:</label>
                            <input type="number" class="form-control" id="taskAchievement-${examId}" value="${exam.criteria.taskAchievement || exam.criteria.taskResponse || ''}" min="0" max="9" step="0.5">
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Coherence and Cohesion:</label>
                            <input type="number" class="form-control" id="coherenceAndCohesion-${examId}" value="${exam.criteria.coherenceAndCohesion || ''}" min="0" max="9" step="0.5">
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Lexical Resource:</label>
                            <input type="number" class="form-control" id="lexicalResource-${examId}" value="${exam.criteria.lexicalResource || ''}" min="0" max="9" step="0.5">
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Grammatical Range and Accuracy:</label>
                            <input type="number" class="form-control" id="grammaticalRange-${examId}" value="${exam.criteria.grammaticalRange || ''}" min="0" max="9" step="0.5">
                        </div>
                        <div class="mb-3">
                            <label class="form-label">Nhận xét:</label>
                            <textarea class="form-control" id="remark-${examId}" rows="3">${exam.remark}</textarea>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Đóng</button>
                        <button type="button" class="btn btn-primary" onclick="saveRemark(${studentId}, '${classroomId}', ${examId})">Lưu</button>
                    </div>
                </div>
            </div>
        </div>
    `;

    // Thêm modal vào body
    document.body.insertAdjacentHTML('beforeend', modalHTML);

    // Hiển thị modal
    const modal = new bootstrap.Modal(document.getElementById(`remarkModal-${examId}`));
    modal.show();
}


// Load classroom section with clickable cards
function loadClassroomSection() {
    let html = `
        <div id="classroomListContainer">
            <h2>Classroom</h2>
            <div class="row mt-4">
        `;

    classrooms.forEach((classroom) => {
        html += `
            <div class="col-md-3">
                <div class="card text-center p-3 mb-4 classroom-card" onclick="showClassroomStudents('${classroom.id}')">
                    <div class="text-center mb-3">
                        <img src="${classroom.img}" alt="${classroom.title}" class="img-fluid" style="max-width: 100px; border-radius: 8px;">
                    </div>
                    <h4>${classroom.title}</h4>
                </div>
            </div>
            `;
    });

    html += `
            </div>
        </div>
        <div id="studentTableContainer" class="d-none">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h3 id="selectedClassroomTitle"></h3>
                <button class="btn btn-outline-secondary" onclick="backToClassrooms()">
                    <i class="fas fa-arrow-left"></i> Quay lại
                </button>
            </div>
            <div id="studentCardContainer" class="row">
            </div>
        </div>`;

    document.getElementById("dynamicContent").innerHTML = html;

    // Add hover effect to classroom cards
    document.querySelectorAll(".classroom-card").forEach((card) => {
        card.style.cursor = "pointer";
        card.style.transition = "transform 0.3s ease, box-shadow 0.3s ease";

        card.addEventListener("mouseenter", () => {
            card.style.transform = "translateY(-5px)";
            card.style.boxShadow = "0 10px 20px rgba(0,0,0,0.1)";
        });

        card.addEventListener("mouseleave", () => {
            card.style.transform = "translateY(0)";
            card.style.boxShadow = "none";
        });
    });
}
// Sửa hàm showStudentDetails để hiển thị dữ liệu theo cấu trúc mới
function showStudentDetails(studentId, classroomId) {
    const student = studentsByClassroom[classroomId].find(
        (s) => s.id === studentId
    );
    if (!student) return;

    let detailContainer = document.getElementById("studentDetailContainer");
    if (!detailContainer) {
        detailContainer = document.createElement("div");
        detailContainer.id = "studentDetailContainer";
        detailContainer.className = "mt-4";
        document.getElementById("dynamicContent").appendChild(detailContainer);
    }

    document.getElementById("studentTableContainer").classList.add("d-none");
    detailContainer.classList.remove("d-none");

    // Lọc các bài kiểm tra (exams) từ các lesson kiểm tra
    const exams = student.lessons
        .filter(lesson => lesson.type === "exam" && lesson.exam)
        .map(lesson => lesson.exam);

    // Tính điểm trung bình của các bài kiểm tra
    const avgExamScore = exams.length > 0
        ? exams.reduce((sum, exam) => sum + exam.score, 0) / exams.length
        : 0;

    // Lọc các điểm quiz từ các lesson học
    const quizScores = student.lessons
        .filter(lesson => lesson.type === "learning" && lesson.quizScore !== undefined)
        .map(lesson => lesson.quizScore);

    // Tính điểm trung bình quiz
    const avgQuizScore = quizScores.length > 0
        ? quizScores.reduce((a, b) => a + b, 0) / quizScores.length
        : 0;

    // Tạo HTML cho bảng điểm kiểm tra
    const examScoresHtml = exams.map((exam, index) => `
        <tr>
            <td class="text-center">${index + 1}</td>
            <td>${exam.title}</td>
            <td class="text-center">${new Date(exam.submittedAt).toLocaleDateString('vi-VN')}</td>
            <td class="text-center">${exam.score}/100</td>
            <td class="text-center">${exam.status === "graded" ? "Đã chấm" : "Chưa chấm"}</td>
            <td class="text-center">
                <button class="btn btn-sm btn-primary remark-btn" onclick="showRemarkModal(${student.id}, '${classroomId}', ${exam.examId})">
                    ${exam.status === "graded" ? "Xem/Sửa" : "Chấm bài"}
                </button>
            </td>
        </tr>
    `).join("");

    const detailsHtml = `
        <div class="container">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h3>Thông tin chi tiết học viên</h3>
                <button class="btn btn-outline-secondary" onclick="backToStudentList('${classroomId}')">
                    <i class="fas fa-arrow-left"></i> Quay lại 
                </button>
            </div>
            
            <div class="row">
                <div class="col-md-4">
                    <div class="card mb-4"> 
                        <div class="card-body text-center">
                            <img src="https://i.pravatar.cc/150?img=${student.id + 10}" alt="${student.name}" class="img-fluid rounded-circle mb-3" style="width: 150px;">
                            <h3>${student.name}</h3>
                            <p class="text-muted">${student.email}</p>
                            <p><i class="fas fa-phone me-2"></i>${student.phone}</p>
                            <div class="d-flex justify-content-center mt-3">
                                <span class="badge bg-info me-2 p-2">Tham gia: ${student.attendance}</span>
                                <span class="badge bg-success p-2">Điểm: ${student.grade}</span>
                            </div>
                        </div>
                    </div>    
                    
                    <div class="card mb-4">
                        <div class="card-header">
                            <h5>Tiến độ học tập</h5>
                        </div>
                        <div class="card-body">
                            <div class="progress mb-3" style="height: 30px;">
                                <div class="progress-bar bg-success" 
                                    role="progressbar" 
                                    style="width: ${(student.completedAssignments / student.totalAssignments) * 100}%;" 
                                    aria-valuenow="${student.completedAssignments}" 
                                    aria-valuemin="0" 
                                    aria-valuemax="${student.totalAssignments}">
                                    ${student.completedAssignments}/${student.totalAssignments} bài tập
                                </div>
                            </div>
                            <div class="progress" style="height: 30px;">
                                <div class="progress-bar bg-info" 
                                    role="progressbar" 
                                    style="width: ${student.attendance.replace("%", "")}%;" 
                                    aria-valuenow="${student.attendance.replace("%", "")}" 
                                    aria-valuemin="0" 
                                    aria-valuemax="100">
                                    Tham gia: ${student.attendance}
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                
                <div class="col-md-8">
                    <div class="card mb-4">
                        <div class="card-header">
                            <h5>Thông tin cá nhân</h5>
                        </div>
                        <div class="card-body">
                            <table class="table table-striped">
                                <tbody>
                                    <tr><td><strong>Họ và tên:</strong></td><td>${student.name}</td></tr>
                                    <tr><td><strong>Ngày sinh:</strong></td><td>${student.dateOfBirth}</td></tr>
                                    <tr><td><strong>Giới tính:</strong></td><td>${student.gender}</td></tr>
                                    <tr><td><strong>Địa chỉ:</strong></td><td>${student.address}</td></tr>
                                    <tr><td><strong>Email:</strong></td><td>${student.email}</td></tr>
                                    <tr><td><strong>Số điện thoại:</strong></td><td>${student.phone}</td></tr>
                                    <tr><td><strong>Ngày tham gia:</strong></td><td>${student.registrationDate}</td></tr>
                                    <tr><td><strong>Đăng nhập gần đây:</strong></td><td>${student.lastLogin}</td></tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                    
                    <div class="row">
                        <div class="col-md-12">
                            <div class="card mb-4">
                                <div class="card-header d-flex justify-content-between align-items-center">
                                    <h5>Điểm kiểm tra</h5>
                                    <button class="btn btn-primary btn-sm" onclick="window.showExamList(${student.id}, '${classroomId}')">
                                        <i class="fas fa-file-alt me-1"></i> Xem bài kiểm tra
                                    </button>
                                </div>
                                <div class="card-body">
                                    <div class="table-responsive">
                                        <table class="table table-striped table-hover exam-table">
                                            <thead>
                                                <tr>
                                                    <th class="text-center" style="width: 5%;">#</th>
                                                    <th style="width: 40%;">Tên bài kiểm tra</th>
                                                    <th class="text-center" style="width: 15%;">Ngày nộp</th>
                                                    <th class="text-center" style="width: 10%;">Điểm</th>
                                                    <th class="text-center" style="width: 15%;">Trạng thái</th>
                                                    <th class="text-center" style="width: 15%;">Hành động</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                ${examScoresHtml}
                                                <tr>
                                                    <td class="text-center"><strong>TB:</strong></td>
                                                    <td colspan="5"><strong>${avgExamScore.toFixed(1)}/100</strong></td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="row">
                        <div class="col-md-12">
                            <div class="card mb-4">
                                <div class="card-header">
                                    <h5>Điểm trung bình quiz</h5>
                                </div>
                                <div class="card-body">
                                    <table class="table table-striped">
                                        <tbody>
                                            <tr>
                                                <td class="text-start" style="font-weight: bold;">Điểm trung bình quiz:</td>
                                                <td class="text-end" style="font-weight: bold;">${avgQuizScore.toFixed(1)}/100</td>
                                            </tr>
                                        </tbody>
                                    </table>  
                                </div>
                            </div>
                        </div>
                    </div>
                    
                    <div class="row">
                        <div class="col-md-12">
                            <div class="card mb-4">
                                <div class="card-header">
                                    <h5>Ghi chú</h5>
                                </div>
                                <div class="card-body">
                                    <p class="text-muted">${student.notes}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <style>
            .exam-table {
                width: 100%;
                table-layout: fixed;
                font-family: 'Roboto', sans-serif;
            }
            .exam-table th, .exam-table td {
                padding: 12px 8px;
                vertical-align: middle;
                border-bottom: 1px solid #dee2e6;
            }
            .exam-table th {
                background-color: #f8f9fa;
                font-weight: 600;
                color: #495057;
            }
            .exam-table tbody tr:hover {
                background-color: #f1f3f5;
            }
            .exam-table .remark-btn {
                width: 100px;
                padding: 6px 12px;
                font-size: 14px;
                white-space: nowrap;
            }
            .card-header {
                background-color: #f8f9fa;
                border-bottom: 1px solid #dee2e6;
            }
            .table-responsive {
                overflow-x: auto;
            }
            .card-body {
                padding: 20px;
            }
        </style>
    `;

    detailContainer.innerHTML = detailsHtml;
}

// Go back from student details to student list
function backToStudentList(classroomId) {
    document.getElementById("studentTableContainer").classList.remove("d-none");
    document.getElementById("studentDetailContainer").classList.add("d-none");
}

// Danh sách khóa học (Demo)
const courses = [
    {
        img: "component/course.jpg",
        name: "JavaScript Basics",
        category: "Programming",
        students: 1200,
    },
    {
        img: "component/course.jpg",
        name: "HTML & CSS Mastery",
        category: "Web Design",
        students: 900,
    },
    {
        img: "component/course.jpg",
        name: "ReactJS for Beginners",
        category: "Frontend",
        students: 1500,
    },
    {
        img: "component/course.jpg",
        name: "Python Data Science",
        category: "Data Science",
        students: 1300,
    },
];

// Thêm vào cuối file script.js
function prepareAndShowClassroom(classroomId) {
    document.getElementById("homePage").classList.add("d-none");
    document.getElementById("dynamicContent").classList.remove("d-none");
    loadClassroomSection();
    setTimeout(() => {
        showClassroomStudents(classroomId);
    }, 100);
}

// Render danh sách khóa học
function loadCourses() {
    let courseList = document.getElementById("courseList");
    courseList.innerHTML = "";
    courses.forEach((course) => {
        let courseHTML = `
            <div class="col-md-3">
                <div class="card course-card p-3">
                    <img src="${course.img}" class="course-img">
                    <h5 class="mt-2">${course.name}</h5>
                    <p class="course-category">${course.category}</p>
                    <p class="course-students">${course.students} học viên</p>
                </div>
            </div>
        `;
        courseList.innerHTML += courseHTML;
    });
}

// Load home page khi trang mở và thêm sự kiện cho nút chuông
document.addEventListener("DOMContentLoaded", () => {
    loadHome();
    const notificationBell = document.querySelector('.btn.btn-light.position-relative');
    if (notificationBell) {
        updateNotificationBadge();
        notificationBell.addEventListener('click', () => {
            showContent('newMessage');
        });
    }
});