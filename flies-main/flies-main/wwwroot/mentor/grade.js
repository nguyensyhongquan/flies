// grade.js - Xử lý các chức năng liên quan đến trang tổng quan điểm số và tiến độ

// Dữ liệu kỹ năng IELTS
const skillsData = [
  {
    name: "Writing Task 1",
    classroomId: "writing1",
    color: "#F0C14B",
    score: 6.5,
    criteria: {
      taskAchievement: 7.0,
      coherenceAndCohesion: 6.5,
      lexicalResource: 5.5,
      grammaticalRange: 6.0
    },
    progress_id: "PRG_SKILL_001",
    enrollement_id: "ENR001",
    completed_lessons: 8,
    completed_quizzes: 4,
    total_lessons: 20,
    total_quizzes: 10,
    progress_percentage: 40,
    updated_at: "2025-03-15T10:30:00Z"
  },
  {
    name: "Writing Task 2", // Thêm mục mới cho Writing Task 2
    classroomId: "writing2",
    color: "#FF6B6B", // Màu mới cho Writing Task 2
    score: 7.0,
    criteria: {
      taskResponse: 7.5,
      coherenceAndCohesion: 7.0,
      lexicalResource: 6.5,
      grammaticalRange: 7.0
    },
    progress_id: "PRG_SKILL_002",
    enrollement_id: "ENR001",
    completed_lessons: 9,
    completed_quizzes: 5,
    total_lessons: 20,
    total_quizzes: 10,
    progress_percentage: 45,
    updated_at: "2025-03-16T09:00:00Z"
  },
  {
    name: "Listening",
    classroomId: "listening",
    color: "#66B2FF",
    score: 5.5,
    criteria: {
      comprehension: 6.0,
      recognition: 5.5,
      accuracy: 5.0,
      attentionToDetail: 5.5
    },
    progress_id: "PRG_SKILL_003",
    enrollement_id: "ENR001",
    completed_lessons: 5,
    completed_quizzes: 3,
    total_lessons: 12,
    total_quizzes: 6,
    progress_percentage: 44.44,
    updated_at: "2025-03-14T12:00:00Z"
  },
  {
    name: "Reading",
    classroomId: "reading",
    color: "#7ED957",
    score: 6.0,
    criteria: {
      accuracy: 6.5,
      speed: 6.0,
      comprehension: 5.5,
      vocabularyUsage: 6.0
    },
    progress_id: "PRG_SKILL_004",
    enrollement_id: "ENR001",
    completed_lessons: 10,
    completed_quizzes: 5,
    total_lessons: 15,
    total_quizzes: 8,
    progress_percentage: 60.87,
    updated_at: "2025-03-12T13:10:00Z"
  }
];

// Dữ liệu nhiệm vụ
const tasksData = [
  {
    id: "writing1",
    title: "Writing Task 1",
    status: 1,
    time: "2 h 20m",
    students: 24,
    progress_id: "PRG_TASK_001",
    enrollement_id: "ENR001",
    completed_lessons: 8,
    completed_quizzes: 4,
    total_lessons: 20,
    total_quizzes: 10,
    progress_percentage: 40,
    updated_at: "2025-03-15T10:30:00Z"
  },
  {
    id: "writing2", // Thêm mục mới cho Writing Task 2
    title: "Writing Task 2",
    status: 2,
    time: "2 h 30m",
    students: 20,
    progress_id: "PRG_TASK_002",
    enrollement_id: "ENR001",
    completed_lessons: 9,
    completed_quizzes: 5,
    total_lessons: 20,
    total_quizzes: 10,
    progress_percentage: 45,
    updated_at: "2025-03-16T09:00:00Z"
  },
  {
    id: "listening",
    title: "Listening",
    status: 3,
    time: "3 h 18m",
    students: 30,
    progress_id: "PRG_TASK_003",
    enrollement_id: "ENR001",
    completed_lessons: 5,
    completed_quizzes: 3,
    total_lessons: 12,
    total_quizzes: 6,
    progress_percentage: 44.44,
    updated_at: "2025-03-14T12:00:00Z"
  },
  {
    id: "reading",
    title: "Reading",
    status: 4,
    time: "1 h 11m",
    students: 22,
    progress_id: "PRG_TASK_004",
    enrollement_id: "ENR001",
    completed_lessons: 10,
    completed_quizzes: 5,
    total_lessons: 15,
    total_quizzes: 8,
    progress_percentage: 60.87,
    updated_at: "2025-03-12T13:10:00Z"
  }
];

// Dữ liệu giáo viên
const teacherData = {
  name: "Nguyễn Thanh Tuấn",
  avatar: "https://i.pravatar.cc/60?img=20",
  description: "Giảng viên IELTS Writing | 5 năm kinh nghiệm"
};

// Hàm giả lập lấy dữ liệu từ database
async function fetchGradeData(studentId, classroomId) {
  return new Promise(resolve => 
    setTimeout(() => {
      resolve({
        skills: skillsData,
        tasks: tasksData,
        teacher: teacherData,
        overallScore: 6.0
      });
    }, 500));
  };
  
// Hàm tạo HTML cho thống kê ở đầu trang
function createHeaderStatsHTML(tasks) {
  tasks = tasks || tasksData;
  const totalStudents = tasks.reduce((sum, task) => sum + task.students, 0);
  const totalHours = tasks.reduce((sum, task) => {
    const timeStr = task.time;
    const hours = parseInt(timeStr.split('h')[0].trim());
    const mins = parseInt(timeStr.split('h')[1].replace('m', '').trim());
    return sum + hours + (mins / 60);
  }, 0).toFixed(1);

  return `
    <div class="header-stats card mb-4 shadow-sm">
      <div class="card-body">
        <div class="row text-center">
          <div class="col-6">
            <div class="stat-item">
              <i class="fas fa-user-graduate fa-2x mb-2 text-primary"></i>
              <h4>${totalStudents}</h4>
              <p class="text-muted mb-0">Tổng số học viên</p>
            </div>
          </div>
          <div class="col-6">
            <div class="stat-item">
              <i class="fas fa-clock fa-2x mb-2 text-success"></i>
              <h4>${totalHours}h</h4>
              <p class="text-muted mb-0">Tổng thời gian học</p>
            </div>
          </div>
        </div>
      </div>
    </div>`;
}

// Khởi tạo biểu đồ kỹ năng IELTS với cơ chế thử lại
function initSkillsChart(canvasId, scores, overallScore, retries = 10, interval = 200) {
  scores = scores || skillsData.map(skill => skill.score);
  overallScore = overallScore || 6.0;

  const canvas = document.getElementById(canvasId);
  if (!canvas) {
    if (retries > 0) {
      console.warn(`Canvas với ID "${canvasId}" không tồn tại trong DOM. Thử lại sau ${interval}ms (${retries} lần còn lại).`);
      setTimeout(() => {
        initSkillsChart(canvasId, scores, overallScore, retries - 1, interval);
      }, interval);
    } else {
      console.error(`Không thể tìm thấy canvas với ID "${canvasId}" sau ${10} lần thử.`);
      console.log("Nội dung hiện tại của dynamicContent:", document.getElementById("dynamicContent").innerHTML);
    }
    return null;
  }

  const ctx = canvas.getContext("2d");

  const data = {
    datasets: [
      {
        data: scores,
        backgroundColor: skillsData.map(skill => skill.color),
        borderWidth: 0,
        weight: 1,
        cutout: "75%",
        circumference: 360,
        rotation: 270,
      },
    ],
  };

  return new Chart(ctx, {
    type: "doughnut",
    data: data,
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { display: false },
        tooltip: { enabled: false },
      },
    },
  });
}

// Tạo HTML cho phần điểm số IELTS (động dựa trên classroomId)
function createSkillScoreHTML(customScores, customOverall, customSkillsData) {
  const skills = customSkillsData || skillsData;
  const scores = customScores || skills.map(skill => skill.score);
  const overallScore = customOverall || 6.0;

  const legendHTML = skills.map((skill, index) => {
    const actualScore = scores[index];
    const criteriaLabels = window.criteriaDefinitions[skill.classroomId]?.labels || {};

    return `
      <div class="d-flex align-items-center mb-3">
        <div style="width: 16px; height: 16px; border-radius: 50%; background-color: ${skill.color}; margin-right: 10px;"></div>
        <div style="flex-grow: 1;">
          <div>${skill.name}</div>
          <small class="text-muted">
            Đã hoàn thành: ${skill.completed_lessons}/${skill.total_lessons} bài học, 
            ${skill.completed_quizzes}/${skill.total_quizzes} bài kiểm tra
          </small>
        </div>
        <span style="font-weight: bold;">${actualScore}</span>
      </div>
      <div class="criteria-details mb-3">
        ${Object.entries(skill.criteria).map(([key, score]) => `
          <div class="row mb-1">
            <div class="col-8">
              <small>${criteriaLabels[key] || key}</small>
            </div>
            <div class="col-4 text-end">
              <small>${score}</small>
            </div>
          </div>
        `).join('')}
      </div>
      <div class="progress mb-3" style="height: 10px;">
        <div class="progress-bar bg-info" role="progressbar" style="width: ${skill.progress_percentage}%"
             aria-valuenow="${skill.progress_percentage}" aria-valuemin="0" aria-valuemax="100">
        </div>
      </div>
      <small class="text-muted mb-3 d-block">
        Cập nhật: ${new Date(skill.updated_at).toLocaleString('vi-VN')}
      </small>`;
  }).join('');

  return `
    <div class="chart-container" style="position: relative; height: 300px; width: 300px; margin: 0 auto;">
      <canvas id="skillsChart"></canvas>
      <div class="center-text" style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); text-align: center;">
        <p style="margin: 0; font-size: 16px;">Overall</p>
        <p style="margin: 0; font-size: 60px; font-weight: bold;">${overallScore}</p>
      </div>
    </div>
    <div class="legend-container mt-4 p-3 bg-light rounded">
      ${legendHTML}
    </div>`;
}

// Tạo HTML cho phần thông tin giáo viên
function createTeacherInfoHTML(teacher) {
  teacher = teacher || teacherData;
  return `
    <div class="teacher-info mb-4 py-4 border-bottom">
      <div class="d-flex align-items-center">
        <img src="${teacher.avatar}" class="rounded-circle me-3" alt="Teacher Avatar">
        <div>
          <h5 class="mb-1">${teacher.name}</h5>
          <p class="text-muted mb-0">${teacher.description}</p>
        </div>
      </div>
    </div>`;
}

// Tạo HTML cho các nhiệm vụ
function createTasksHTML(tasks) {
  tasks = tasks || tasksData;
  return tasks.map(task => `
    <div class="task-card">
      <div class="task-info">
        <div class="status-indicator">${task.status}</div>
        <div>
          <h3 class="task-title">${task.title}</h3>
          <div class="students-info">
            <i class="fas fa-user-graduate"></i> ${task.students} học viên
          </div>
          <small class="text-muted">
            Đã hoàn thành: ${task.completed_lessons}/${task.total_lessons} bài học, 
            ${task.completed_quizzes}/${task.total_quizzes} bài kiểm tra
          </small>
          <div class="progress mt-2" style="height: 10px;">
            <div class="progress-bar bg-info" role="progressbar" style="width: ${task.progress_percentage}%"
                 aria-valuenow="${task.progress_percentage}" aria-valuemin="0" aria-valuemax="100">
            </div>
          </div>
          <small class="text-muted d-block mt-1">
            Cập nhật: ${new Date(task.updated_at).toLocaleDateString('vi-VN')}
          </small>
        </div>
      </div>
      <div class="task-meta">
        <span class="task-time"><i class="far fa-clock"></i> ${task.time}</span>
        <button class="view-btn" onclick="prepareAndShowClassroom('${task.id}')">Chi tiết</button>
      </div>
    </div>`).join('');
}

// Tạo HTML cho phần tổng quan lớp học
function createClassOverviewHTML(tasks) {
  tasks = tasks || tasksData;
  const avgProgress = (tasks.reduce((sum, task) => sum + task.progress_percentage, 0) / tasks.length).toFixed(1);

  return `
    <div class="class-overview card mb-4">
      <div class="card-body">
        <div class="row text-center">
          <div class="col-12">
            <div class="stat-item">
              <i class="fas fa-chart-line fa-2x mb-2 text-info"></i>
              <h4>${avgProgress}%</h4>
              <p class="text-muted">Tiến độ trung bình</p>
            </div>
          </div>
        </div>
      </div>
    </div>`;
}

// Tạo template đầy đủ cho phần điểm số
async function getGradeTemplate(studentId, classroomId) {
  const data = await fetchGradeData(studentId, classroomId);

  const scores = data.skills.map(skill => skill.score);

  return `
    <div class="container py-4">
      <div class="mb-4">
        <h2 class="m-0">Overview</h2>
        ${createHeaderStatsHTML(data.tasks)}
      </div>
      
      <div class="row">
        <div class="col-md-6">
          ${createClassOverviewHTML(data.tasks)}
          <div class="card shadow-sm">
            <div class="card-body pt-4">
              ${createSkillScoreHTML(scores, data.overallScore, data.skills)}
            </div>
          </div>
        </div>
        
        <div class="col-md-6">
          <div class="task-container card shadow-sm">
            ${createTeacherInfoHTML(data.teacher)}
            <h5 class="px-3 mb-3">Classroom</h5>
            <div class="task-list">
              ${createTasksHTML(data.tasks)}
            </div>
          </div>
        </div>
      </div>
    </div>`;
}

// Khởi tạo tất cả các biểu đồ trong trang grade
function initGradeCharts() {
  const scores = skillsData.map(skill => skill.score);
  initSkillsChart("skillsChart", scores);
}

// Hàm cập nhật điểm số từ examDetails.js
function updateGradeData(studentId, classroomId, examId, newScore, newCriteria) {
  const skill = skillsData.find(s => s.progress_id === `PRG_SKILL_${examId.toString().padStart(3, '0')}`);
  if (skill) {
    skill.score = newScore;
    skill.criteria = newCriteria;
    skill.updated_at = new Date().toISOString();
    if (document.getElementById("skillsChart")) {
      initGradeCharts();
    }
  }
}

// Thêm CSS cho UI cải tiến
function addCustomStyles() {
  const styleSheet = document.createElement("style");
  styleSheet.textContent = `
    .header-stats {
      background-color: #f8f9fa;
      border-radius: 8px;
    }
    .task-card {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px;
      border-bottom: 1px solid #e9ecef;
      transition: all 0.2s ease;
    }
    .task-card:hover {
      background-color: #f8 W9fa;
      transform: translateY(-2px);
      box-shadow: 0 4px 6px rgba(0,0,0,0.05);
    }
    .task-info {
      display: flex;
      align-items: flex-start;
      gap: 15px;
    }
    .status-indicator {
      width: 30px;
      height: 30px;
      border-radius: 50%;
      background-color: #f0f0f0;
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: bold;
      color: white;
      flex-shrink: 0;
    }
    .status-indicator:nth-child(1) {
      background-color: #F0C14B;
    }
    .task-card:nth-child(odd) .status-indicator {
      background-color: #FF6B6B;
    }
    .task-card:nth-child(3) .status-indicator {
      background-color: #66B2FF;
    }
    .task-card:nth-child(4) .status-indicator {
      background-color: #7ED957;
    }
    .task-title {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
    }
    .students-info {
      color: #6c757d;
      font-size: 14px;
      margin-top: 4px;
      display: flex;
      align-items: center;
      gap: 5px;
    }
    .task-meta {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 8px;
    }
    .task-time {
      color: #6c757d;
      font-size: 14px;
    }
    .view-btn {
      background-color: #007bff;
      color: white;
      border: none;
      border-radius: 4px;
      padding: 6px 12px;
      font-size: 14px;
      cursor: pointer;
      transition: background-color 0.2s;
    }
    .view-btn:hover {
      background-color: #0069d9;
    }
    .teacher-info {
      background-color: #f8f9fa;
      border-radius: 8px;
      padding: 16px !important;
      margin-bottom: 20px !important;
    }
    .chart-container {
      box-shadow: 0 4px 6px rgba(0,0,0,0.05);
      border-radius: 8px;
      padding: 16px;
    }
    .legend-container {
      background-color: #f8f9fa;
      border-radius: 8px;
      padding: 16px;
      margin-top: 20px !important;
    }
    .progress {
      height: 1rem;
    }
    .progress-bar {
      font-size: 0.8rem;
    }
    .criteria-details .row {
      align-items: center;
    }
  `;
  document.head.appendChild(styleSheet);
}

// Thêm hàm khởi tạo UI
function initUI() {
  addCustomStyles();
  initGradeCharts(); // Gọi initGradeCharts sau khi HTML đã render
  if (!document.querySelector('link[href*="font-awesome"]')) {
    const fontAwesome = document.createElement('link');
    fontAwesome.rel = 'stylesheet';
    fontAwesome.href = 'https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css';
    document.head.appendChild(fontAwesome);
  }
}

// Export các hàm cần thiết
window.getGradeTemplate = getGradeTemplate;
window.initGradeCharts = initGradeCharts;
window.initUI = initUI;
window.skillsData = skillsData;
window.tasksData = tasksData;
window.teacherData = teacherData;
window.updateGradeData = updateGradeData;