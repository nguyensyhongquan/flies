document.addEventListener("DOMContentLoaded", async () => {
    await startSignalRConnection();
    loadChatBox();
});

// Khởi tạo SignalR connection với token xác thực
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub", {
        accessTokenFactory: () => localStorage.getItem('token')
    })
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Biến toàn cục để lưu trữ thông tin user và chat group (dùng cho SignalR reconnect)
let currentStudentId = null;
let currentMentorId = null;
let currentUser = null;

// Bộ nhớ để tránh hiển thị tin nhắn trùng lặp
const displayedMessages = new Set();
async function startSignalRConnection() {
    try {
        if (connection.state !== signalR.HubConnectionState.Disconnected) {
            await connection.stop();
            await new Promise(resolve => setTimeout(resolve, 500));
        }
        await connection.start();
        console.log("SignalR Connected.");
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        setTimeout(startSignalRConnection, 5000);
    }
}

async function startSignalR(studentId, mentorId) {
    try {
        // Check the current state of the connection
        if (connection.state !== signalR.HubConnectionState.Disconnected) {
            await connection.stop();
            // Add a small delay to ensure the connection is fully stopped
            await new Promise(resolve => setTimeout(resolve, 500));
        }

        await connection.start();
        console.log("SignalR Connected.");

        const parsedStudentId = parseInt(studentId);
        const parsedMentorId = parseInt(mentorId);
        await connection.invoke("JoinChat", parsedStudentId, parsedMentorId);

        // Lưu thông tin để reconnect nếu cần
        currentStudentId = parsedStudentId;
        currentMentorId = parsedMentorId;
    } catch (err) {
        console.error("SignalR Connection Error:", err);
        // Only attempt reconnection if the connection is disconnected
        setTimeout(() => {
            if (connection.state === signalR.HubConnectionState.Disconnected) {
                startSignalR(studentId, mentorId);
            } else {
                console.log(`Skipping reconnection attempt. Connection is in ${connection.state} state.`);
            }
        }, 5000);
    }
}

connection.onclose(async () => {
    console.log("SignalR Disconnected. Attempting to reconnect...");
    if (connection.state === signalR.HubConnectionState.Disconnected && currentStudentId && currentMentorId) {
        await startSignalR(currentStudentId, currentMentorId);
    } else {
        console.log(`Cannot reconnect. Connection is in ${connection.state} state or missing studentId/mentorId.`);
        // Optionally force stop the connection and retry
        await connection.stop();
        await new Promise(resolve => setTimeout(resolve, 500)); // Wait for stop to complete
        if (currentStudentId && currentMentorId) {
            await startSignalR(currentStudentId, currentMentorId);
        }
    }
});

function openChatWithStudent(studentName, messageId) {
    markMessageAsRead(messageId);
    showContent('chatBox', studentName);
}

async function showContent(contentId, selectedStudent = null) {
    if (contentId === "myStudent") {
        loadClassroomSection();
    } else if (contentId === "chatBox") {
        if (typeof window.loadChatBox === "function") {
            window.loadChatBox(selectedStudent);
        } else {
            console.error("Hàm loadChatBox chưa được định nghĩa. Kiểm tra file message.js");
            document.getElementById("dynamicContent").innerHTML = "<h2>Lỗi</h2><p>Không thể tải Chat Box. Vui lòng kiểm tra lại.</p>";
        }
    } else {
        let content;
        if (contentId === "grade") {
            content = await window.getGradeTemplate("student123", "classroom456");
        } else {
            content = getContentTemplate(contentId);
        }
        console.log(`Đang render nội dung cho contentId: ${contentId}`);
        document.getElementById("dynamicContent").innerHTML = content;
        console.log("Nội dung dynamicContent sau khi render:", document.getElementById("dynamicContent").innerHTML);

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

        if (contentId === "newMessage") {
            document.querySelectorAll('.status-btn').forEach(btn => {
                btn.addEventListener('click', (e) => {
                    e.stopPropagation();
                    const messageId = parseInt(btn.dataset.messageId);
                    markMessageAsRead(messageId);
                });
            });
        }
    }
}

async function loadChatBox(selectedStudent = null) {
    try {
        // Lấy danh sách học viên hoặc mentor từ API
        const studentResponse = await fetch('/api/Messages/students', {
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
        });
        if (!studentResponse.ok) throw new Error('Không thể lấy danh sách học viên');
        const studentList = await studentResponse.json();

        // Lấy thông tin người dùng hiện tại
        const userResponse = await fetch('/api/Messages/current', {
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
        });
        if (!userResponse.ok) throw new Error('Không thể lấy thông tin người dùng');
        currentUser = await userResponse.json();

        // Nếu không có selectedStudent, lấy học viên đầu tiên từ studentList
        if (!selectedStudent && studentList.length > 0) {
            selectedStudent = studentList[0].name;
        }

        // Tìm id tương ứng với selectedStudent
        const selectedId = studentList.find(s => s.name === selectedStudent)?.id || 0;
        if (selectedId === 0) {
            console.error('Không tìm thấy ID cho học viên hoặc mentor:', selectedStudent);
            document.getElementById("dynamicContent").innerHTML = "<h2>Lỗi</h2><p>Không tìm thấy học viên hoặc mentor.</p>";
            return;
        }

        // Xác định StudentId và MentorId cho SignalR
        const studentId = currentUser.role === 'student' ? currentUser.id : selectedId;
        const mentorId = currentUser.role === 'mentor' ? currentUser.id : selectedId;

        // Kết nối SignalR và tham gia nhóm chat
        await startSignalR(studentId, mentorId);

        // Lấy tin nhắn từ API với tham số id
        const messageResponse = await fetch(`/api/Messages?id=${selectedId}`, {
            headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
        });
        if (!messageResponse.ok) throw new Error('Không thể lấy tin nhắn');
        const messages = await messageResponse.json();

        // Hiển thị tên người nhận
        const displayName = currentUser.role === 'student' ? studentList.find(s => s.id === selectedId)?.name || 'Không xác định' : selectedStudent;

        // Xóa bộ nhớ tin nhắn đã hiển thị để tải lại toàn bộ tin nhắn mới
        displayedMessages.clear();

        // Tạo HTML cho giao diện chat
        let html = `
            <div class="chat-container">
                <div class="row g-0">
                    <div class="col-md-4 student-list">
                        <h5 class="p-3 border-bottom">Danh sách ${currentUser.role === 'student' ? 'giáo viên' : 'học viên'}</h5>
                        ${studentList.map(student => `
                            <div class="student-item ${selectedStudent === student.name ? 'active' : ''}" data-student="${student.name}">
                                <img src="https://i.pravatar.cc/40?img=${student.id}" alt="${student.name}">
                                <span>${student.name}</span>
                            </div>
                        `).join('')}
                    </div>
                    <div class="col-md-8 chat-area">
                        <div class="chat-header">
                            <img src="https://i.pravatar.cc/40?img=${studentList.find(s => s.name === selectedStudent)?.id || 1}" alt="${displayName || 'Không xác định'}">
                            <h5 class="mb-0">Cuộc trò chuyện với ${displayName || 'Không xác định'}</h5>
                        </div>
                        <div class="chat-messages" id="chatMessages">
                            ${generateChatContent(messages, currentUser)}
                        </div>
                        <div class="chat-input">
                            <input type="text" id="chatInput" placeholder="Nhập tin nhắn...">
                            <button onclick="sendMessage('${displayName}', '${currentUser.role}', '${currentUser.fullname}', ${selectedId}, ${studentId}, ${mentorId})">Gửi</button>
                        </div>
                    </div>
                </div>
            </div>
        `;

        const dynamicContent = document.getElementById("dynamicContent");
        if (dynamicContent) {
            dynamicContent.innerHTML = html;

            const studentItems = document.querySelectorAll(".student-item");
            studentItems.forEach((item) => {
                item.addEventListener("click", async () => {
                    studentItems.forEach((i) => i.classList.remove("active"));
                    item.classList.add("active");
                    const studentName = item.dataset.student;
                    // Rời nhóm chat hiện tại
                    await connection.invoke("LeaveChat", studentId, mentorId).catch(err => {
                        console.error("Error leaving chat group:", err);
                    });
                    loadChatBox(studentName);
                });
            });

            const chatMessages = document.getElementById("chatMessages");
            if (chatMessages) {
                chatMessages.scrollTop = chatMessages.scrollHeight;
            }

            const chatInput = document.getElementById("chatInput");
            if (chatInput) {
                chatInput.addEventListener("keypress", (e) => {
                    if (e.key === "Enter") {
                        sendMessage(displayName, currentUser.role, currentUser.fullname, selectedId, studentId, mentorId);
                    }
                });
            }
        }

        // Xử lý tin nhắn nhận được từ SignalR
        connection.on("ReceiveMessage", (receivedStudentId, receivedMentorId, sender, content, time) => {
            if (receivedStudentId === studentId && receivedMentorId === mentorId) {
                console.log("Received message:", { sender, content, time });
                addMessageToChat(sender, content, time, receivedStudentId, receivedMentorId, currentUser);
            }
        });
    } catch (error) {
        console.error("Lỗi khi tải Chat Box:", error);
        document.getElementById("dynamicContent").innerHTML = "<h2>Lỗi</h2><p>Không thể tải Chat Box. Vui lòng thử lại.</p>";
    }
}

function generateChatContent(messages, currentUser) {
    return messages
        .map(msg => {
            const isOwnMessage = (currentUser.role === 'mentor' && msg.sender.toLowerCase() === 'mentor') ||
                (currentUser.role === 'student' && msg.sender === currentUser.fullname);
            const messageClass = isOwnMessage ? 'own-message' : 'other-message';
            // Tạo key duy nhất để tránh trùng lặp khi load lịch sử tin nhắn
            const messageKey = `${msg.sender}:${msg.content}:${msg.time}`;
            displayedMessages.add(messageKey);
            return `
                <div class="chat-message ${messageClass}">
                    <div class="chat-bubble ${messageClass}">
                        ${msg.content}
                    </div>
                    <div class="time">${msg.time}</div>
                </div>
            `;
        })
        .join("");
}

function addMessageToChat(sender, content, time, studentId, mentorId, currentUser) {
    // Tạo key duy nhất để kiểm tra tin nhắn trùng lặp
    const messageKey = `${sender}:${content}:${time}`;

    // Chỉ thêm tin nhắn nếu chưa được hiển thị
    if (!displayedMessages.has(messageKey)) {
        displayedMessages.add(messageKey);
        const isOwnMessage = (currentUser.role === 'mentor' && sender.toLowerCase() === 'mentor') ||
            (currentUser.role === 'student' && sender === currentUser.fullname);
        const messageClass = isOwnMessage ? 'own-message' : 'other-message';
        const messageHtml = `
            <div class="chat-message ${messageClass}">
                <div class="chat-bubble ${messageClass}">
                    ${content}
                </div>
                <div class="time">${time}</div>
            </div>
        `;
        const chatMessages = document.getElementById("chatMessages");
        if (chatMessages) {
            chatMessages.insertAdjacentHTML('beforeend', messageHtml);
            chatMessages.scrollTop = chatMessages.scrollHeight;
        }
    } else {
        console.log("Duplicate message ignored:", messageKey);
    }
}

async function sendMessage(studentName, userRole, userFullname, recipientId, studentId, mentorId) {
    const chatInput = document.getElementById("chatInput");
    const messageContent = chatInput.value.trim();
    if (!messageContent) return;

    try {
        // Gửi tin nhắn qua API
        const response = await fetch('/api/Messages', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + localStorage.getItem('token')
            },
            body: JSON.stringify({
                studentId: parseInt(recipientId),
                content: messageContent
            })
        });

        if (!response.ok) throw new Error('Lỗi khi gửi tin nhắn qua API');
        chatInput.value = "";
    } catch (error) {
        console.error("Lỗi khi gửi tin nhắn:", error);
    }
}

window.loadChatBox = loadChatBox;
window.sendMessage = sendMessage;
