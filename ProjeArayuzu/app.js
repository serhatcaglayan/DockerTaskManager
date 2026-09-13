// Docker üzerindeki API adresimiz
const API_URL = 'http://localhost:8080/api/projects'; 
const loadBtn = document.getElementById('loadBtn');
const container = document.getElementById('projectsContainer');

// Butona tıklandığında çalışacak olay dinleyicisi
loadBtn.addEventListener('click', async () => {
    try {
        // Butona tıklandığında kullanıcıya bilgi verelim
        container.innerHTML = '<p style="text-align: center;">Yükleniyor...</p>';

        // API'ye GET isteği atıyoruz
        const response = await fetch(API_URL);
        
        if (!response.ok) {
            throw new Error(`Sunucu Hatası: ${response.status}`);
        }

        // Gelen JSON verisini JavaScript dizisine çeviriyoruz
        const projects = await response.json();
        
        // Verileri ekrana çizdiren fonksiyonu çağırıyoruz
        renderProjects(projects);

    } catch (error) {
        container.innerHTML = `<p style="color: red; text-align: center;">Hata oluştu: ${error.message}</p>`;
    }
});

// Projeleri HTML'e çeviren fonksiyon
function renderProjects(projects) {
    container.innerHTML = ''; // İçeriği temizle

    if (projects.length === 0) {
        container.innerHTML = '<p style="text-align: center;">Henüz hiç proje eklenmemiş.</p>';
        return;
    }

    // Her bir proje için döngü oluştur
    projects.forEach(project => {
        const card = document.createElement('div');
        card.className = 'project-card';

        // Projenin içindeki görevleri (TaskItems) listele
        let tasksHtml = '';
        if (project.taskItems && project.taskItems.length > 0) {
            tasksHtml = '<ul class="task-list">';
            project.taskItems.forEach(task => {
                const statusIcon = task.isCompleted ? '✅' : '⏳';
                tasksHtml += `<li class="task-item">${statusIcon} ${task.title}</li>`;
            });
            tasksHtml += '</ul>';
        } else {
            tasksHtml = '<p style="color: #888; font-size: 0.9em;">Bu projeye ait görev yok.</p>';
        }

        // Kartın içine başlık, açıklama ve görevleri ekle
// Kartın içine başlık, açıklama, görevler ve YENİ GÖREV EKLEME ALANI ekle
        card.innerHTML = `
            <h3 class="project-title">${project.name}</h3>
            <p>${project.description}</p>
            <strong>Görevler:</strong>
            ${tasksHtml}
            
            <!-- YENİ EKLENEN KISIM -->
            <div style="margin-top: 15px; display: flex; gap: 10px;">
                <input type="text" id="taskInput-${project.id}" placeholder="Yeni görev..." style="flex: 1; padding: 5px;">
                <button onclick="addTask(${project.id})" style="margin: 0; padding: 5px 10px;">Ekle</button>
            </div>
        `;
        container.appendChild(card);
    });
}

const addForm = document.getElementById('addProjectForm');

// Form gönderildiğinde çalışacak kod
addForm.addEventListener('submit', async (e) => {
    e.preventDefault(); // Sayfanın yenilenmesini engelle

    // 1. Kutulardaki yazıları alıp bir nesne yapıyoruz
    const newProject = {
        name: document.getElementById('projectName').value,
        description: document.getElementById('projectDesc').value
    };

    // 2. API'ye POST isteği atıyoruz (Sihir burada)
    await fetch(API_URL, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newProject)
    });

    // 3. Formu temizle ve listeyi yenilemek için gizlice GET butonuna tıkla
    addForm.reset();
    loadBtn.click(); 
});

// Yeni görev ekleme fonksiyonu (HTML içindeki butondan tetiklenir)
async function addTask(projectId) {
    const input = document.getElementById(`taskInput-${projectId}`);
    
    // Kutu boşsa uyarı verme, direkt işlemi iptal et (basitlik için)
    if (!input.value) return; 

    const newTask = {
        title: input.value,
        description: "", // Basit tutmak için boş gönderiyoruz
        isCompleted: false,
        projectId: projectId // Hangi projeye ait olduğunu otomatik alıyor
    };

    // API'nin Görevler (TaskItems) ucuna POST isteği atıyoruz
    await fetch('http://localhost:8080/api/taskitems', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newTask)
    });

    // Listeyi yenilemek için gizlice 'Verileri Getir' butonuna tıkla
    document.getElementById('loadBtn').click(); 
}