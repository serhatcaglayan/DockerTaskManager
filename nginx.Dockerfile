FROM nginx:alpine

# Kendi conf dosyamızı, Nginx'in orijinal dosyasının üzerine yazıyoruz
COPY nginx/nginx.conf /etc/nginx/nginx.conf