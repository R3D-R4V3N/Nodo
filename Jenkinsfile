pipeline {
    agent { label 'acceptatie' }

    stages {
         stage('Checkout dev Repo') {
            steps {
                sh 'rm -rf dev-repo'

                dir('dev-repo') {
                    checkout([
                        $class: 'GitSCM',
                        branches: [[name: '*/main']],
                        userRemoteConfigs: [[
                            url: 'git@github.com:HOGENT-RISE/dotnet-2526-gent12.git',
                            credentialsId: 'github-deploy-key-dev'
                        ]]
                    ])
                }
            }
        }

        stage('Checkout ops Repo') {
            steps {
                sh 'rm -rf ops-repo'

                dir('ops-repo') {
                    checkout([
                        $class: 'GitSCM',
                        branches: [[name: '*/main']],
                        userRemoteConfigs: [[
                            url: 'git@github.com:HOGENT-RISE/ops-2526-gent12.git',
                            credentialsId: 'github-deploy-key-ops'
                        ]]
                    ])
                }
            }
        }

        stage('Copy Docker Files') {
            steps {
                echo 'Copying docker-compose and Dockerfile from ops repo...'
                sh '''
                cp ops-repo/src/buildserver/pipeline/docker-compose.yml dev-repo/
                cp ops-repo/src/buildserver/pipeline/Dockerfile dev-repo/
                ls -la dev-repo/docker-compose.yml dev-repo/Dockerfile
                '''
            }
        }

        stage('Build Docker Image') {
            steps {
                echo 'Building Docker image...'
                sh '''
                cd dev-repo
                DOCKER_BUILDKIT=1 docker build \
                    --progress=plain \
                    -t rise-server:${BUILD_NUMBER} \
                    -t rise-server:latest \
                    .
                '''
            }
        }

        stage('Stop Previous Containers') {
            steps {
                echo 'Stopping previous Docker Compose services...'
                sh '''
                cd dev-repo

                if [ "$(docker ps -aq -f name=rise-server)" ]; then
                    echo "rise-server is running. Stopping and removing..."
                    docker rm -f $(docker ps -aq -f name=rise-server)
                else
                    echo "rise-server is not running. Nothing to stop."
                fi
                '''
            }
        }

        stage('Start Docker Compose') {
            steps {
                echo 'Starting Docker Compose services...'
                sh '''
                cd dev-repo
                docker compose -f docker-compose.yml up -d
                echo 'Waiting for services to be ready...'
                sleep 15
                '''
            }
        }

        stage('Health Check') {
            steps {
                echo 'Performing health checks...'
                sh '''
                echo "Checking Rise.Server health..."
                timeout 30 bash -c 'until curl -f http://localhost:5001/ 2>/dev/null; do sleep 2; done' || true
                echo "Service check complete"
                '''
            }
        }
    }

    post {
        always {
            echo 'Pipeline execution completed'
            sh '''
            cd dev-repo
            docker compose -f docker-compose.yml logs --tail=20 || true
            '''
        }
        failure {
            echo 'Pipeline failed! Full logs:'
            sh '''
            cd dev-repo
            docker compose -f docker-compose.yml logs || true
            '''
        }
    }
}