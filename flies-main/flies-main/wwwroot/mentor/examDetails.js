// examDetails.js - Xử lý chức năng hiển thị bài kiểm tra của học viên

// Dữ liệu mẫu (sẽ thay bằng dữ liệu từ database sau này)
const examData = {
  "writing1": {
    1: [
      {
        id: 1,
        progress_id: "PRG001",
        enrollement_id: "ENR001",
        title: "Writing Task 1",
        date: "15/03/2025",
        score: 6.5,
        completed_lessons: 8,
        completed_quizzes: 4,
        total_lessons: 20,
        total_quizzes: 10,
        progress_percentage: 40,
        updated_at: "2025-03-15T10:30:00Z",
        image: "https://i.ibb.co/3YxvRq9/ielts-writing-task1.jpg",
        content: `It is evident from the charts that the global demand for water witnessed a remarkable increase from 1900 to 2000...`,
        feedback: [
          {
            title: "Góp ý & chấm điểm",
            criteria: {
              taskAchievement: 7.0,
              coherenceAndCohesion: 6.5,
              lexicalResource: 5.5,
              grammaticalRange: 6.0
            },
            points: [
              "Cụm \"witnessed a remarkable increase\" có thể thay bằng \"experienced a significant increase\"...",
              "Cụm \"Fast forward to 2000\" hơi mang tính văn nói...",
              "Cụm \"Brazil's possession of 265 times more irrigated land\" nên viết lại..."
            ]
          }
        ]
      }
    ]
  },
  "reading": {
    1: [
      {
        id: 4,
        progress_id: "PRG004",
        enrollement_id: "ENR001",
        title: "Reading Passage 1",
        date: "12/03/2025",
        score: 6.0,
        completed_lessons: 10,
        completed_quizzes: 5,
        total_lessons: 15,
        total_quizzes: 8,
        progress_percentage: 60.87,
        updated_at: "2025-03-12T13:10:00Z",
        image: "https://i.ibb.co/3YxvRq9/ielts-reading.jpg",
        content: `The passage discusses the impact of climate change on global agriculture...`,
        feedback: [
          {
            title: "Góp ý & chấm điểm",
            criteria: {
              accuracy: 6.5,
              speed: 6.0,
              comprehension: 5.5,
              vocabularyUsage: 6.0
            },
            points: [
              "Đáp án câu 3 chưa chính xác, cần đọc kỹ đoạn 2.",
              "Tốc độ đọc ổn nhưng cần cải thiện khả năng hiểu chi tiết.",
              "Sử dụng từ vựng tốt nhưng cần đa dạng hơn."
            ]
          }
        ]
      }
    ]
  },
  "listening": {
    1: [
      {
        id: 3,
        progress_id: "PRG003",
        enrollement_id: "ENR001",
        title: "Listening Section 1",
        date: "14/03/2025",
        score: 5.5,
        completed_lessons: 5,
        completed_quizzes: 3,
        total_lessons: 12,
        total_quizzes: 6,
        progress_percentage: 44.44,
        updated_at: "2025-03-14T12:00:00Z",
        image: "https://i.ibb.co/3YxvRq9/ielts-listening.jpg",
        content: `The audio clip is a conversation between two students discussing their study plans...`,
        feedback: [
          {
            title: "Góp ý & chấm điểm",
            criteria: {
              comprehension: 6.0,
              recognition: 5.5,
              accuracy: 5.0,
              attentionToDetail: 5.5
            },
            points: [
              "Hiểu được ý chính nhưng bỏ sót một số chi tiết quan trọng.",
              "Cần cải thiện khả năng nhận diện từ vựng trong ngữ cảnh.",
              "Đáp án câu 5 sai do không chú ý đến số liệu."
            ]
          }
        ]
      }
    ]
  }
};

// Hàm giả lập lấy dữ liệu từ database
async function fetchExams(studentId, classroomId) {
  return new Promise(resolve => 
    setTimeout(() => {
      resolve(examData[classroomId]?.[studentId] || []);
    }, 500));
  };


// Hàm giả lập cập nhật dữ liệu vào database
async function updateExamFeedback(studentId, classroomId, examId, updatedData) {
  return new Promise(resolve => 
    setTimeout(() => {
      const exams = examData[classroomId][studentId];
      const examIndex = exams.findIndex(exam => exam.id === parseInt(examId));
      if (examIndex !== -1) {
        exams[examIndex] = { ...exams[examIndex], ...updatedData };
        resolve({ success: true });
      } else {
        resolve({ success: false, error: "Không tìm thấy bài kiểm tra" });
      }
    }, 500));
  };


// Hàm tính Band Score từ các tiêu chí (động)
function calculateBandScore(criteria) {
  const scores = Object.values(criteria);
  const average = scores.reduce((sum, score) => sum + score, 0) / scores.length;
  return Math.round(average * 2) / 2; // Làm tròn đến 0.5 gần nhất
}

// Hàm tiện ích để tạo hoặc lấy container
function ensureContainer(containerId) {
  let container = document.getElementById(containerId);
  if (!container) {
    container = document.createElement("div");
    container.id = containerId;
    container.className = "container mt-4";
    document.getElementById("dynamicContent").appendChild(container);
  }
  return container;
}

// Hàm ẩn các container khác
function hideOtherContainers(activeContainer) {
  const containers = ["studentTableContainer", "studentDetailContainer", "examListContainer", "examDetailContainer"];
  containers.forEach(id => {
    const container = document.getElementById(id);
    if (container && container !== activeContainer) {
      container.classList.add("d-none");
    }
  });
  activeContainer.classList.remove("d-none");
}

// Hàm tạo HTML cho phần grade
function createGradeHTML(exam) {
  return `
    <div class="grade-info p-3 bg-light rounded mb-3">
      <h6 class="mb-3">Thông tin điểm số</h6>
      <div class="row">
        <div class="col-md-6">
          <p><strong>Band Score:</strong> ${exam.score}</p>
          <p><strong>Mã tiến độ:</strong> ${exam.progress_id}</p>
          <p><strong>Mã đăng ký:</strong> ${exam.enrollement_id}</p>
          <p><strong>Cập nhật lúc:</strong> ${new Date(exam.updated_at).toLocaleString('vi-VN')}</p>
        </div>
        <div class="col-md-6">
          <p><strong>Bài học:</strong> ${exam.completed_lessons}/${exam.total_lessons}</p>
          <p><strong>Bài kiểm tra:</strong> ${exam.completed_quizzes}/${exam.total_quizzes}</p>
          <div class="progress mt-2" style="height: 20px;">
            <div class="progress-bar bg-info" role="progressbar" style="width: ${exam.progress_percentage}%"
                 aria-valuenow="${exam.progress_percentage}" aria-valuemin="0" aria-valuemax="100">
              ${exam.progress_percentage}%
            </div>
          </div>
        </div>
      </div>
    </div>
  `;
}

// Hàm tạo HTML cho phần feedback (động dựa trên classroomId)
function createFeedbackHTML(feedback, isEditable = false, examId, classroomId) {
  const criteriaLabels = window.criteriaDefinitions[classroomId]?.labels || {};

  return `
    <div class="card mb-4">
      <div class="card-header d-flex justify-content-between align-items-center">
        <h5 class="m-0">${feedback.title}</h5>
        ${isEditable ? `
          <div>
            <button class="btn btn-sm btn-success me-2" onclick="saveFeedback(${examId}, '${classroomId}')">
              <i class="fas fa-save"></i> Lưu
            </button>
            <button class="btn btn-sm btn-secondary" onclick="cancelEdit(${examId}, '${classroomId}')">
              <i class="fas fa-times"></i> Hủy
            </button>
          </div>
        ` : `
          <button class="btn btn-sm btn-primary" onclick="startEditFeedback(${examId}, '${classroomId}')">
            <i class="fas fa-edit"></i> Sửa
          </button>
        `}
      </div>
      <div class="card-body">
        <div class="criteria-scores mb-4">
          ${Object.entries(feedback.criteria).map(([key, score]) => `
            <div class="row mb-2">
              <div class="col-8">
                <strong>${criteriaLabels[key] || key}</strong>
              </div>
              <div class="col-4 text-end">
                ${isEditable ? `
                  <input type="number" step="0.5" min="0" max="9" 
                         class="form-control form-control-sm d-inline-block w-auto criteria-score" 
                         data-key="${key}" value="${score}">
                ` : `
                  <span>${score}</span>
                `}
              </div>
            </div>
          `).join('')}
        </div>
        <h6 class="mb-3">Nhận xét</h6>
        <ul class="list-group list-group-flush" id="feedback-points-${examId}">
          ${isEditable ? feedback.points.map((point, index) => `
            <li class="list-group-item d-flex align-items-center">
              <textarea class="form-control me-2 feedback-point" data-index="${index}">${point}</textarea>
              <button class="btn btn-sm btn-danger" onclick="removeFeedbackPoint(${examId}, ${index})">
                <i class="fas fa-trash"></i>
              </button>
            </li>
          `).join('') : feedback.points.map(point => `
            <li class="list-group-item">
              <i class="fas fa-comment text-primary me-2"></i> ${point}
            </li>
          `).join('')}
        </ul>
        ${isEditable ? `
          <button class="btn btn-sm btn-outline-primary mt-3" onclick="addFeedbackPoint(${examId})">
            <i class="fas fa-plus"></i> Thêm nhận xét
          </button>
        ` : ''}
      </div>
    </div>
  `;
}

// Hàm hiển thị bài kiểm tra
async function showExamDetails(studentId, classroomId, examId = null) {
  if (!examId) {
    showExamList(studentId, classroomId);
    return;
  }

  const exams = await fetchExams(studentId, classroomId);
  const exam = exams.find(exam => exam.id === parseInt(examId));

  let examDetailContainer = ensureContainer("examDetailContainer");
  if (!exam) {
    examDetailContainer.innerHTML = `
      <div class="alert alert-warning">
        Không tìm thấy bài kiểm tra với ID ${examId}.
      </div>
    `;
    return;
  }

  hideOtherContainers(examDetailContainer);

  const isEditable = exam.isEditing || false;

  let examDetailHTML = `
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3>${exam.title}</h3>
      <button class="btn btn-outline-secondary" onclick="backToExamList(${studentId}, '${classroomId}')">
        <i class="fas fa-arrow-left"></i> Quay lại
      </button>
    </div>

    <div class="row">
      <div class="col-md-8">
        <div class="card mb-4">
          <div class="card-header">
            <h5 class="m-0">Bài làm</h5>
          </div>
          <div class="card-body">
            ${createGradeHTML(exam)}
            <div class="text-center mb-4">
              <img src="${exam.image}" alt="${exam.title}" class="img-fluid border" style="max-height: 300px;" loading="lazy">
            </div>
            ${exam.content && !exam.answers ? `
              <div class="mt-3">
                <p>${exam.content.replace(/\n\n/g, '</p><p>').replace(/\n/g, '<br>')}</p>
              </div>
            ` : ''}
            ${exam.answers ? `
              <div class="mt-3">
                <p>${exam.content}</p>
                <div class="table-responsive">
                  <table class="table table-striped">
                    <thead>
                      < hw
                        <th>Câu hỏi</th>
                        <th>Câu trả lời</th>
                        <th>Kết quả</th>
                      </tr>
                    </thead>
                    <tbody>
                      ${exam.answers.map(answer => `
                        <tr>
                          <td>Question ${answer.question}</td>
                          <td>${answer.answer}</td>
                          <td>
                            ${answer.correct
                              ? '<span class="text-success"><i class="fas fa-check-circle"></i> Đúng</span>'
                              : `<span class="text-danger"><i class="fas fa-times-circle"></i> Sai</span><br>
                                 <small class="text-muted">Đáp án đúng: ${answer.correctAnswer}</small>`
                            }
                          </td>
                        </tr>
                      `).join('')}
                    </tbody>
                  </table>
                </div>
              </div>
            ` : ''}
          </div>
        </div>
      </div>

      <div class="col-md-4">
        <div class="card mb-4">
          <div class="card-header">
            <h5 class="m-0">Thông tin bài kiểm tra</h5>
          </div>
          <div class="card-body">
            <ul class="list-group list-group-flush">
              <li class="list-group-item d-flex justify-content-between">
                <span>Tiêu đề:</span>
                <span class="text-muted">${exam.title}</span>
              </li>
              <li class="list-group-item d-flex justify-content-between">
                <span>Ngày làm bài:</span>
                <span class="text-muted">${exam.date}</span>
              </li>
            </ul>
          </div>
        </div>
        ${exam.feedback && exam.feedback.length > 0 ? exam.feedback.map(feedback => createFeedbackHTML(feedback, isEditable, examId, classroomId)).join('') : `
          <div class="card mb-4">
            <div class="card-header">
              <h5 class="m-0">Góp ý & chấm điểm</h5>
            </div>
            <div class="card-body">
              <p class="text-muted">Chưa có góp ý.</p>
              <button class="btn btn-sm btn-primary" onclick="startEditFeedback(${examId}, '${classroomId}')">
                <i class="fas fa-plus"></i> Thêm góp ý
              </button>
            </div>
          </div>
        `}
      </div>
    </div>
  `;

  examDetailContainer.innerHTML = examDetailHTML;
}

// Hàm bắt đầu chỉnh sửa feedback
function startEditFeedback(examId, classroomId) {
  const exams = examData[classroomId][1];
  const exam = exams.find(exam => exam.id === parseInt(examId));
  if (exam) {
    exam.isEditing = true;
    showExamDetails(1, classroomId, examId);
  }
}

// Hàm thêm điểm góp ý mới
function addFeedbackPoint(examId) {
  const feedbackList = document.getElementById(`feedback-points-${examId}`);
  const newIndex = feedbackList.children.length;
  const newPoint = `
    <li class="list-group-item d-flex align-items-center">
      <textarea class="form-control me-2 feedback-point" data-index="${newIndex}"></textarea>
      <button class="btn btn-sm btn-danger" onclick="removeFeedbackPoint(${examId}, ${newIndex})">
        <i class="fas fa-trash"></i>
      </button>
    </li>
  `;
  feedbackList.insertAdjacentHTML('beforeend', newPoint);
}

// Hàm xóa điểm góp ý
function removeFeedbackPoint(examId, index) {
  const feedbackList = document.getElementById(`feedback-points-${examId}`);
  const pointElement = feedbackList.querySelector(`.feedback-point[data-index="${index}"]`).parentElement;
  pointElement.remove();
  feedbackList.querySelectorAll('.feedback-point').forEach((el, i) => {
    el.setAttribute('data-index', i);
  });
}

// Hàm lưu feedback
async function saveFeedback(examId, classroomId) {
  const feedbackList = document.getElementById(`feedback-points-${examId}`);
  const points = Array.from(feedbackList.querySelectorAll('.feedback-point')).map(el => el.value.trim()).filter(val => val);

  const criteriaInputs = document.querySelectorAll('.criteria-score');
  const criteria = {};
  criteriaInputs.forEach(input => {
    const key = input.getAttribute('data-key');
    const value = parseFloat(input.value);
    if (!isNaN(value) && value >= 0 && value <= 9) {
      criteria[key] = value;
    }
  });

  const newScore = calculateBandScore(criteria);

  const updatedData = {
    feedback: [{
      title: "Góp ý & chấm điểm",
      criteria: criteria,
      points: points
    }],
    score: newScore,
    updated_at: new Date().toISOString()
  };

  const result = await updateExamFeedback(1, classroomId, examId, updatedData);
  if (result.success) {
    const exams = examData[classroomId][1];
    const exam = exams.find(exam => exam.id === parseInt(examId));
    exam.isEditing = false;
    if (window.updateGradeData) {
      window.updateGradeData(1, classroomId, examId, newScore, criteria);
    }
    showExamDetails(1, classroomId, examId);
  } else {
    alert("Lỗi khi lưu góp ý: " + (result.error || "Không xác định"));
  }
}

// Hàm hủy chỉnh sửa
function cancelEdit(examId, classroomId) {
  const exams = examData[classroomId][1];
  const exam = exams.find(exam => exam.id === parseInt(examId));
  if (exam) {
    exam.isEditing = false;
    showExamDetails(1, classroomId, examId);
  }
}

// Hiển thị danh sách bài kiểm tra của học viên
async function showExamList(studentId, classroomId) {
  const exams = await fetchExams(studentId, classroomId);
  let examListContainer = ensureContainer("examListContainer");
  hideOtherContainers(examListContainer);

  let examListHTML = `
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3>Danh sách bài kiểm tra</h3>
      <button class="btn btn-outline-secondary" onclick="backToStudentDetails(${studentId}, '${classroomId}')">
        <i class="fas fa-arrow-left"></i> Quay lại thông tin học viên
      </button>
    </div>
    
    <div class="row">
      ${exams.map(exam => `
        <div class="col-md-4 mb-4">
          <div class="card h-100 exam-card" onclick="showExamDetails(${studentId}, '${classroomId}', ${exam.id})">
            <div class="card-header">
              <h5 class="m-0">${exam.title}</h5>
            </div>
            <div class="card-body">
              <div class="grade-info p-2 bg-light rounded mb-3">
                <p class="mb-1"><strong>Band Score:</strong> ${exam.score}</p>
                <p class="mb-1"><strong>Tiến độ:</strong> ${exam.progress_percentage}%</p>
                <div class="progress mt-1" style="height: 10px;">
                  <div class="progress-bar bg-info" role="progressbar" style="width: ${exam.progress_percentage}%"
                       aria-valuenow="${exam.progress_percentage}" aria-valuemin="0" aria-valuemax="100">
                  </div>
                </div>
                <small class="text-muted d-block mt-1">
                  ${exam.completed_lessons}/${exam.total_lessons} bài học, 
                  ${exam.completed_quizzes}/${exam.total_quizzes} bài kiểm tra
                </small>
              </div>
              <div class="text-center mb-3">
                <img src="${exam.image}" alt="${exam.title}" class="img-fluid" style="max-height: 120px;" loading="lazy">
              </div>
              <p class="text-muted mb-0">Ngày làm bài: ${exam.date}</p>
              <p class="text-muted mb-0">Cập nhật: ${new Date(exam.updated_at).toLocaleDateString('vi-VN')}</p>
            </div>
          </div>
        </div>
      `).join('')}
    </div>
  `;

  examListContainer.innerHTML = examListHTML;

  examListContainer.addEventListener("mouseenter", (e) => {
    const card = e.target.closest(".exam-card");
    if (card) {
      card.style.transform = "translateY(-5px)";
      card.style.boxShadow = "0 10px 20px rgba(0,0,0,0.1)";
    }
  }, true);

  examListContainer.addEventListener("mouseleave", (e) => {
    const card = e.target.closest(".exam-card");
    if (card) {
      card.style.transform = "translateY(0)";
      card.style.boxShadow = "none";
    }
  }, true);
}

// Quay lại danh sách bài kiểm tra
function backToExamList(studentId, classroomId) {
  showExamList(studentId, classroomId);
}

// Quay lại trang thông tin chi tiết học viên
function backToStudentDetails(studentId, classroomId) {
  const examListContainer = document.getElementById("examListContainer");
  const examDetailContainer = document.getElementById("examDetailContainer");
  const studentDetailContainer = document.getElementById("studentDetailContainer");
  if (examListContainer) examListContainer.classList.add("d-none");
  if (examDetailContainer) examDetailContainer.classList.add("d-none");
  if (studentDetailContainer) studentDetailContainer.classList.remove("d-none");
}

// Lấy danh sách bài kiểm tra của học viên (cho tích hợp ngoài)
function getExamsForStudent(studentId, classroomId) {
  return examData[classroomId]?.[studentId] || [];
}

// Xác định màu của badge dựa trên điểm band
function getBandScoreClass(score) {
  if (score >= 7.5) return "success";
  if (score >= 6.5) return "info";
  if (score >= 5.5) return "warning";
  return "danger";
}

// Thêm CSS cho các phần tử mới
function addExamStyles() {
  const styleSheet = document.createElement("style");
  styleSheet.textContent = `
    .exam-card {
      transition: transform 0.3s ease, box-shadow 0.3s ease;
      cursor: pointer;
    }
    .progress {
      height: 1rem;
    }
    .progress-bar {
      font-size: 0.8rem;
    }
    .grade-info {
      border: 1px solid #e9ecef;
    }
    .feedback-point {
      resize: vertical;
      min-height: 60px;
    }
    .criteria-scores .row {
      align-items: center;
    }
    .criteria-score {
      width: 80px;
    }
  `;
  document.head.appendChild(styleSheet);
}

// Khởi tạo styles khi trang được load
document.addEventListener("DOMContentLoaded", addExamStyles);

// Export các hàm cần thiết
window.showExamDetails = showExamDetails;
window.showExamList = showExamList;
window.backToExamList = backToExamList;
window.backToStudentDetails = backToStudentDetails;
window.getExamsForStudent = getExamsForStudent;
window.startEditFeedback = startEditFeedback;
window.addFeedbackPoint = addFeedbackPoint;
window.removeFeedbackPoint = removeFeedbackPoint;
window.saveFeedback = saveFeedback;
window.cancelEdit = cancelEdit;