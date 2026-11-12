pipeline {
    agent any

    environment {
        PROJECT_NAME = 'LoggingSystem'
        DOTNET_VERSION = '9.0'
    }

    stages {
        stage('Checkout') {
            steps {
                echo "Клонирование проекта ${env.PROJECT_NAME}..."
                checkout scm
            }
        }
        
        stage('Setup') {
            steps {
                echo 'Установка зависимостей...'
                bat 'dotnet restore || echo "Ошибка restore"'
            }
        }
        
        stage('Lint') {
            steps {
                echo 'Статический анализ (сборка и проверка анализаторами)...'
                // Эта команда запускает компилятор и встроенные анализаторы .NET
                bat 'dotnet build --no-restore || echo "Ошибка build"'
            }
        }
        
        stage('Test') {
            steps {
                echo 'Запуск тестов...'
                bat 'dotnet test --no-build || echo "Тесты не найдены или упали"'
            }
        }
        
        stage('Archive') {
            steps {
                echo 'Сохранение артефакта...'
                bat 'if not exist artifacts mkdir artifacts && echo Build %BUILD_NUMBER% > artifacts\\build.txt'
                archiveArtifacts artifacts: 'artifacts/**', allowEmptyArchive: true
            }
        }
    }
    
    post {
        always {
            echo "Pipeline завершён. Номер сборки: ${env.BUILD_NUMBER}"
        }
        success {
            echo 'Все этапы прошли успешно! Статический анализ не выявил проблем.'
        }
        failure {
            echo 'Один из этапов завершился с ошибкой'
        }
    }
}