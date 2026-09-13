pipeline {
    agent any
    
    stages {
        stage('Kodu İndir') {
            steps {
                // GitHub'daki en güncel kodu Jenkins'in içine çeker
                checkout scm
            }
        }
        
        stage('Test Kontrolü') {
            steps {
                // CI (Sürekli Entegrasyon) kısmını GitHub Actions'a bıraktık. 
                // Burada CD (Sürekli Dağıtım) yapıyoruz.
                echo 'GitHub Actions testleri doğruladı, Deploy işlemine geçiliyor...'
            }
        }
        
        stage('Deploy (Canlıya Al)') {
            steps {
                // Jenkins içindeki Docker CLI, bilgisayarındaki Docker'a komut gönderir.
                // Eski konteynerleri durdurur, yeni kodlarla imajı derler ve sistemi başlatır!
                sh 'docker compose -f docker-compose.yml up -d --build'
            }
        }
    }
}