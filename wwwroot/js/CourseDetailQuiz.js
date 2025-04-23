// Quiz modal handling
$(function () {
    // Click event for quiz cards
    $('.quiz-card').on('click', function () {
        const quizId = $(this).data('quiz-id');
        const quizType = $(this).data('quiz-type');
        const lessonId = $(this).data('lesson-id');

        // Update modal title and content
        $('#quizModalLabel').text('Đang tải quiz...');
        $('.quiz-content').html(`
            <div class="d-flex justify-content-center">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
            </div>
        `);

        // Add quiz type badge
        let badgeClass = 'bg-primary';
        if (quizType.toLowerCase() === 'reading') {
            badgeClass = 'bg-info';
        } else if (quizType.toLowerCase() === 'listening') {
            badgeClass = 'bg-warning';
        } else if (quizType.toLowerCase() === 'writing') {
            badgeClass = 'bg-success';
        }

        $('.quiz-type-badge').html(`<span class="badge ${badgeClass}">${quizType}</span>`);

        // Get quiz data via AJAX
        $.ajax({
            url: `/CourseDetailQuiz/GetQuizInfo/${quizId}`,
            method: 'GET',
            success: function (data) {
                // Update modal title
                $('#quizModalLabel').text(data.title);

                // Show submit button
                $('.quiz-submit-btn').show();

                // Display quiz content based on type
                let quizContent = '';
                if (data.quizType.toLowerCase() === 'reading') {
                    // Display reading quiz
                    quizContent = renderReadingQuiz(data);
                } else if (data.quizType.toLowerCase() === 'listening') {
                    // Display listening quiz
                    quizContent = renderListeningQuiz(data);
                } else if (data.quizType.toLowerCase() === 'writing') {
                    // Display writing quiz
                    quizContent = renderWritingQuiz(data);
                } else {
                    // Default quiz display
                    quizContent = renderDefaultQuiz(data);
                }

                // Update modal content
                $('.quiz-content').html(quizContent);
            },
            error: function () {
                // Show error message
                $('#quizModalLabel').text('Lỗi khi tải quiz');
                $('.quiz-content').html(`
                    <div class="alert alert-danger">
                        Không thể tải nội dung quiz. Vui lòng thử lại sau.
                    </div>
                `);
                $('.quiz-submit-btn').hide();
            }
        });
    });

    // For demonstration - these functions would render the different quiz types
    function renderReadingQuiz(data) {
        return `
            <div class="reading-quiz">
                <div class="reading-passage border p-3 mb-3 bg-light">
                    ${data.content || 'Nội dung bài đọc sẽ hiển thị ở đây.'}
                </div>
                <div class="questions-container">
                    ${renderQuestions(data.quizQuestions)}
                </div>
            </div>
        `;
    }

    function renderListeningQuiz(data) {
        return `
            <div class="listening-quiz">
                <div class="audio-player mb-3">
                    ${data.mediaUrl ? `
                        <audio controls class="w-100">
                            <source src="${data.mediaUrl}" type="audio/mpeg">
                            Your browser does not support the audio element.
                        </audio>
                    ` : 'Audio không khả dụng.'}
                </div>
                <div class="questions-container">
                    ${renderQuestions(data.quizQuestions)}
                </div>
            </div>
        `;
    }

    function renderWritingQuiz(data) {
        return `
            <div class="writing-quiz">
                <div class="prompt-container border p-3 mb-3 bg-light">
                    ${data.content || 'Đề bài viết sẽ hiển thị ở đây.'}
                </div>
                <div class="form-group">
                    <label for="writingAnswer">Bài viết của bạn:</label>
                    <textarea class="form-control" id="writingAnswer" rows="10" placeholder="Viết bài của bạn ở đây..."></textarea>
                </div>
            </div>
        `;
    }

    function renderDefaultQuiz(data) {
        return `
            <div class="default-quiz">
                <div class="questions-container">
                    ${renderQuestions(data.quizQuestions)}
                </div>
            </div>
        `;
    }

    function renderQuestions(questions) {
        if (!questions || questions.length === 0) {
            return `<p>Không có câu hỏi nào.</p>`;
        }

        let questionsHtml = '';
        questions.forEach((question, index) => {
            questionsHtml += `
                <div class="question-item mb-4">
                    <div class="question-text fw-bold mb-2">${index + 1}. ${question.questionText}</div>
                    ${renderAnswers(question)}
                </div>
            `;
        });

        return questionsHtml;
    }

    function renderAnswers(question) {
        if (!question.quizAnswers || question.quizAnswers.length === 0) {
            return `<p>Không có câu trả lời nào.</p>`;
        }

        let answersHtml = '';
        const questionType = question.questionType.toLowerCase();

        if (questionType === 'single_choice') {
            question.quizAnswers.forEach((answer) => {
                answersHtml += `
                <div class="form-check">
                    <input class="form-check-input" type="radio" name="question${question.questionId}" id="answer${answer.answerId}" value="${answer.answerId}">
                    <label class="form-check-label" for="answer${answer.answerId}">
                        ${answer.answerText}
                    </label>
                </div>
            `;
            });
        } else if (questionType === 'multiple_choice') {
            question.quizAnswers.forEach((answer) => {
                answersHtml += `
                <div class="form-check">
                    <input class="form-check-input" type="checkbox" name="question${question.questionId}" id="answer${answer.answerId}" value="${answer.answerId}">
                    <label class="form-check-label" for="answer${answer.answerId}">
                        ${answer.answerText}
                    </label>
                </div>
            `;
            });
        } else if (questionType === 'true_false') {
            answersHtml += `
            <div class="form-check">
                <input class="form-check-input" type="radio" name="question${question.questionId}" id="answer_true_${question.questionId}" value="true">
                <label class="form-check-label" for="answer_true_${question.questionId}">
                    Đúng
                </label>
            </div>
            <div class="form-check">
                <input class="form-check-input" type="radio" name="question${question.questionId}" id="answer_false_${question.questionId}" value="false">
                <label class="form-check-label" for="answer_false_${question.questionId}">
                    Sai
                </label>
            </div>
        `;
        } else if (questionType === 'writing') {
            answersHtml = `
            <div class="form-group">
                <textarea class="form-control" name="question${question.questionId}" rows="5" placeholder="Nhập câu trả lời của bạn"></textarea>
            </div>
        `;
        }

        return answersHtml;
    }

    // Submit quiz button handler
    $('.quiz-submit-btn').on('click', function () {
        // Placeholder - implement actual quiz submission later
        alert('Tính năng nộp bài chưa được triển khai.');
    });
});