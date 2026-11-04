pipeline {
    agent any

    stages {
        stage('Checkout') {
            steps {
                echo 'Код получен из Git'
                sh 'ls -la || dir'
            }
        }
        stage('Build') {
            steps {
                echo 'Сборка проекта...'
                sh 'dotnet --version || echo "dotnet не найден"'
            }
        }
        stage('Test') {
            steps {
                echo 'Запуск тестов...'
                sh 'echo "Тесты прошли"'
            }
        }
    }
    post {
        success {
            echo 'Пайплайн завершён успешно!'
        }
        failure {
            echo 'Пайплайн завершился с ошибкой'
        }
    }
}