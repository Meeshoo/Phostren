source "docker" "aspnet" {
  image = "mcr.microsoft.com/dotnet/aspnet:8.0"
  commit = true
  changes = [
    "ENV FOO bar",
    "EXPOSE 80",
    "WORKDIR /phostren",
    "CMD [\"/phostren/phostren.dll\"]",
    "ENTRYPOINT [\"/usr/bin/dotnet\"]"
  ]
}

build {
  sources = ["source.docker.aspnet"]

  provisioner "shell" {
    inline = ["mkdir /phostren"]
  }

  provisioner "file" {
    source = "../output/"
    destination = "/phostren"
  }

  post-processors {
    post-processor "docker-tag" {
      repository = "550661752655.dkr.ecr.eu-west-1.amazonaws.com/phostren-backend"
      tags       = ["latest"]
    }

    post-processor "docker-push" {
      ecr_login = true
      login_server = "https://550661752655.dkr.ecr.eu-west-1.amazonaws.com/mitlan"
    }
  }
}
