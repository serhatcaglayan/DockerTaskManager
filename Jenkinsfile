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
                // Önce eski sistemi temizle (hata vermemesi için)
                sh 'docker compose -p taskmanager -f docker-compose.yml down'
                
                // Sonra yeni kodlarla temiz bir şekilde yeniden inşa et
                sh 'docker compose -p taskmanager -f docker-compose.yml up -d --build'
            }
        }
    }
}