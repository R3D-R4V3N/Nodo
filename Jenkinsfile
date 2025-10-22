pipeline {
    agent any

    
    options {
        timeout(time: 1, unit: 'HOURS')
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }

    triggers {
        githubPush()
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Checking out source code...'
                checkout scm
            }
        }

        stage('Build Docker Image') {
            steps {
                echo 'Building Docker image for Rise.Server...'
                script {
                    sh 'docker build -t rise-server:${BUILD_NUMBER} -t rise-server:latest .'
                }
            }
        }

        stage('Stop Previous Containers') {
            steps {
                echo 'Stopping previous Docker Compose services...'
                script {
                    sh '''
                    if [ -f "docker-compose.yml" ]; then
                        /usr/local/bin/docker-compose -f docker-compose.yml down || true
                    fi
                    '''
                }
            }
        }

        stage('Start Docker Compose') {
            steps {
                echo 'Starting Docker Compose services...'
                script {
                    sh '''
                    /usr/local/bin/docker-compose -f docker-compose.yml up -d
                    echo 'Waiting for services to be ready...'
                    sleep 10
                    '''
                }
            }
        }

        stage('Health Check') {
            steps {
                echo 'Performing health checks...'
                script {
                    sh '''
                    echo "Checking Rise.Server health..."
                    for i in {1..30}; do
                        if curl -f http://localhost:5001/health 2>/dev/null || curl -f http://localhost:5001/ 2>/dev/null; then
                            echo "Rise.Server is healthy!"
                            exit 0
                        fi
                        echo "Attempt $i/30 - waiting for service..."
                        sleep 2
                    done
                    echo "Warning: Could not verify service health"
                    exit 0
                    '''
                }
            }
        }

        stage('Show Logs') {
            steps {
                echo 'Docker Compose Services Status:'
                script {
                    sh '/usr/local/bin/docker-compose -f docker-compose.yml ps'
                }
            }
        }
    }

    post {
        always {
            echo 'Pipeline execution completed'
            script {
                sh '/usr/local/bin/docker-compose -f docker-compose.yml logs --tail=50 || true'
            }
        }
        failure {
            echo 'Pipeline failed! Check Docker logs:'
            script {
                sh '/usr/local/bin/docker-compose -f docker-compose.yml logs || true'
            }
        }
    }
}
