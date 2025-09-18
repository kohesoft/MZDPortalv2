// Grafikleri oluşturmak için gereken değişkenler
let charts = {};
let performanceData = {
    cpu: { current: 0, history: [] },
    ram: { current: 0, total: 16384, history: [] }, // MB cinsinden
    disk: { current: 250, total: 500, history: [] }, // GB cinsinden
    sessions: { current: 0, history: [] },
    loadTime: { current: 0, history: [] },
    requests: { current: 0, history: [] },
    traffic: []
};

// Zaman dilimlerini oluşturma
const hours = Array.from({ length: 24 }, (_, i) => `${i}:00`);

// Sayfa yüklendiğinde çalışacak fonksiyon
document.addEventListener('DOMContentLoaded', function () {
    initCharts();
    setupRealTimeData();
});

// Grafikleri başlatma
function initCharts() {
    const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        scales: {
            x: {
                display: true,
                title: {
                    display: false,
                }
            },
            y: {
                display: true,
                beginAtZero: true,
            }
        },
        plugins: {
            legend: {
                display: false,
            }
        }
    };

    // CPU Chart
    const cpuCtx = document.getElementById('cpuChart').getContext('2d');
    charts.cpu = new Chart(cpuCtx, {
        type: 'line',
        data: {
            labels: getTimeLabels(10),
            datasets: [{
                label: 'CPU Kullanımı (%)',
                data: Array(10).fill(0),
                borderColor: 'rgba(59, 130, 246, 1)',
                backgroundColor: 'rgba(59, 130, 246, 0.2)',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: chartOptions
    });

    // RAM Chart
    const ramCtx = document.getElementById('ramChart').getContext('2d');
    charts.ram = new Chart(ramCtx, {
        type: 'line',
        data: {
            labels: getTimeLabels(10),
            datasets: [{
                label: 'RAM Kullanımı (MB)',
                data: Array(10).fill(0),
                borderColor: 'rgba(16, 185, 129, 1)',
                backgroundColor: 'rgba(16, 185, 129, 0.2)',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: chartOptions
    });

    // Disk Chart
    const diskCtx = document.getElementById('diskChart').getContext('2d');
    charts.disk = new Chart(diskCtx, {
        type: 'line',
        data: {
            labels: getTimeLabels(10),
            datasets: [{
                label: 'Disk Kullanımı (GB)',
                data: Array(10).fill(0),
                borderColor: 'rgba(139, 92, 246, 1)',
                backgroundColor: 'rgba(139, 92, 246, 0.2)',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: chartOptions
    });

    // Sessions Chart
    const sessionsCtx = document.getElementById('sessionsChart').getContext('2d');
    charts.sessions = new Chart(sessionsCtx, {
        type: 'line',
        data: {
            labels: getTimeLabels(10),
            datasets: [{
                label: 'Aktif Oturumlar',
                data: Array(10).fill(0),
                borderColor: 'rgba(245, 158, 11, 1)',
                backgroundColor: 'rgba(245, 158, 11, 0.2)',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: chartOptions
    });

    // Load Time Chart
    const loadTimeCtx = document.getElementById('loadTimeChart').getContext('2d');
    charts.loadTime = new Chart(loadTimeCtx, {
        type: 'line',
        data: {
            labels: getTimeLabels(10),
            datasets: [{
                label: 'Sayfa Yükleme Süresi (ms)',
                data: Array(10).fill(0),
                borderColor: 'rgba(239, 68, 68, 1)',
                backgroundColor: 'rgba(239, 68, 68, 0.2)',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: chartOptions
    });

    // Requests Chart
    const requestsCtx = document.getElementById('requestsChart').getContext('2d');
    charts.requests = new Chart(requestsCtx, {
        type: 'line',
        data: {
            labels: getTimeLabels(10),
            datasets: [{
                label: 'İstek Sayısı (/sn)',
                data: Array(10).fill(0),
                borderColor: 'rgba(79, 70, 229, 1)',
                backgroundColor: 'rgba(79, 70, 229, 0.2)',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: chartOptions
    });

    // Traffic Chart
    const trafficCtx = document.getElementById('trafficChart').getContext('2d');
    charts.traffic = new Chart(trafficCtx, {
        type: 'bar',
        data: {
            labels: hours,
            datasets: [{
                label: 'Günlük Trafik (istek)',
                data: Array(24).fill(0),
                backgroundColor: 'rgba(59, 130, 246, 0.7)',
                borderColor: 'rgba(59, 130, 246, 1)',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true,
                    title: {
                        display: true,
                        text: 'İstek Sayısı'
                    }
                },
                x: {
                    title: {
                        display: true,
                        text: 'Saat'
                    }
                }
            },
            plugins: {
                legend: {
                    display: true,
                }
            }
        }
    });
}

// Son 10 zaman etiketi oluşturma
function getTimeLabels(count) {
    const labels = [];
    for (let i = count - 1; i >= 0; i--) {
        const d = new Date();
        d.setMinutes(d.getMinutes() - i);
        labels.push(d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
    }
    return labels;
}

// Gerçek zamanlı veri akışı
function setupRealTimeData() {
    // İlk veri alımı
    fetchPerformanceData();

    // Her 5 saniyede bir güncelle
    setInterval(fetchPerformanceData, 5000);
}

// Sunucudan performans verilerini al
function fetchPerformanceData() {
    fetch('/Performance/GetPerformanceData')
        .then(response => response.json())
        .then(data => {
            if (data.error) {
                console.error('Veri alınamadı:', data.error);
                return;
            }

            // CPU verilerini güncelle
            performanceData.cpu.current = data.Cpu.toFixed(1);
            document.querySelector('.cpu-usage').textContent = `${performanceData.cpu.current}%`;
            document.getElementById('cpu-bar').style.width = `${performanceData.cpu.current}%`;
            performanceData.cpu.history.push(performanceData.cpu.current);
            if (performanceData.cpu.history.length > 10) performanceData.cpu.history.shift();
            updateChart('cpu', performanceData.cpu.history);

            // RAM verilerini güncelle
            const ramUsedMB = Math.floor(data.Memory.Used / (1024 * 1024));
            const ramTotalMB = Math.floor(data.Memory.Total / (1024 * 1024));
            performanceData.ram.current = ramUsedMB;
            performanceData.ram.total = ramTotalMB;
            document.querySelector('.ram-usage').textContent = `${(ramUsedMB / 1024).toFixed(1)} GB / ${(ramTotalMB / 1024).toFixed(1)} GB`;
            document.getElementById('ram-bar').style.width = `${(ramUsedMB / ramTotalMB) * 100}%`;
            performanceData.ram.history.push(ramUsedMB);
            if (performanceData.ram.history.length > 10) performanceData.ram.history.shift();
            updateChart('ram', performanceData.ram.history);

            // Disk verilerini güncelle
            const diskUsedGB = Math.floor(data.Disk.Used / (1024 * 1024 * 1024));
            const diskTotalGB = Math.floor(data.Disk.Total / (1024 * 1024 * 1024));
            performanceData.disk.current = diskUsedGB;
            performanceData.disk.total = diskTotalGB;
            document.querySelector('.disk-usage').textContent = `${diskUsedGB} GB / ${diskTotalGB} GB`;
            document.getElementById('disk-bar').style.width = `${(diskUsedGB / diskTotalGB) * 100}%`;
            performanceData.disk.history.push(diskUsedGB);
            if (performanceData.disk.history.length > 10) performanceData.disk.history.shift();
            updateChart('disk', performanceData.disk.history);

            // Oturum verilerini güncelle
            performanceData.sessions.current = data.Sessions.Active;
            document.querySelector('.active-sessions').textContent = performanceData.sessions.current;
            performanceData.sessions.history.push(performanceData.sessions.current);
            if (performanceData.sessions.history.length > 10) performanceData.sessions.history.shift();
            updateChart('sessions', performanceData.sessions.history);

            // Sayfa yükleme süresi verilerini güncelle
            performanceData.loadTime.current = data.Performance.LoadTime;
            document.querySelector('.load-time').textContent = `${performanceData.loadTime.current} ms`;
            performanceData.loadTime.history.push(performanceData.loadTime.current);
            if (performanceData.loadTime.history.length > 10) performanceData.loadTime.history.shift();
            updateChart('loadTime', performanceData.loadTime.history);

            // İstek sayısı verilerini güncelle
            performanceData.requests.current = data.Performance.RequestsPerSecond;
            document.querySelector('.requests-count').textContent = `${performanceData.requests.current} / sn`;
            performanceData.requests.history.push(performanceData.requests.current);
            if (performanceData.requests.history.length > 10) performanceData.requests.history.shift();
            updateChart('requests', performanceData.requests.history);

            // Sunucu bilgilerini güncelle
            document.getElementById('server-name').textContent = data.ServerInfo.Hostname;
            document.getElementById('server-os').textContent = `${data.ServerInfo.Platform} ${data.ServerInfo.Version}`;
            document.getElementById('server-uptime').textContent = formatUptime(data.ServerInfo.Uptime);
            document.getElementById('server-ip').textContent = data.ServerInfo.IpAddress;
            document.getElementById('server-cpu').textContent = data.ServerInfo.CpuModel;
            document.getElementById('server-ram').textContent = `${(data.Memory.Total / (1024 * 1024 * 1024)).toFixed(1)} GB`;

            // Sistem durumu göstergelerini güncelle
            updateServiceStatus('server', data.Services.WebServer);
            updateServiceStatus('db', data.Services.Database);
            updateServiceStatus('cache', data.Services.Cache);

            // Trafik verilerini gncelle
            charts.traffic.data.datasets[0].data = data.Traffic;
            charts.traffic.update();
        })
        .catch(error => {
            console.error('Veri alınamadı:', error);
        });
}

// Grafikleri güncelleme
function updateChart(chartName, data) {
    const chart = charts[chartName];

    // Trafik grafiği için ayrı işlem
    if (chartName === 'traffic') {
        chart.data.datasets[0].data = data;
        chart.update();
        return;
    }

    // Diğer grafikler
    chart.data.datasets[0].data = data;
    chart.data.labels = getTimeLabels(data.length);
    chart.update();
}

// Uptime formatla
function formatUptime(uptimeObj) {
    const days = uptimeObj.Days;
    const hours = uptimeObj.Hours;
    const minutes = uptimeObj.Minutes;

    return `${days} gün, ${hours} saat, ${minutes} dakika`;
}

// Servis durumu güncelle
function updateServiceStatus(prefix, isRunning) {
    const statusEl = document.getElementById(`${prefix}-status`);
    const indicatorEl = document.getElementById(`${prefix}-status-indicator`);

    if (!statusEl || !indicatorEl) return;

    if (isRunning) {
        statusEl.textContent = 'Çalışıyor';
        statusEl.classList.remove('text-red-500');
        statusEl.classList.add('text-green-500');

        indicatorEl.classList.remove('bg-red-500');
        indicatorEl.classList.add('bg-green-500');
    } else {
        statusEl.textContent = 'Kapalı';
        statusEl.classList.remove('text-green-500');
        statusEl.classList.add('text-red-500');

        indicatorEl.classList.remove('bg-green-500');
        indicatorEl.classList.add('bg-red-500');
    }
}
