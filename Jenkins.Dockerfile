FROM jenkins/jenkins:lts
USER root

# Jenkins içine Docker komut satırı araçlarını yüklüyoruz
RUN curl -fsSL https://get.docker.com/ | sh

USER jenkins