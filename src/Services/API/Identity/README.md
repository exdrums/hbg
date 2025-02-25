- API.Identity projects contain whole logic for implementation of an OIDC server, with admin and admin.api services to manage the Identity Server. It base of IdentityServer4 Project
- Create Docker Images from the root folder of API.Identity projects. 
    e.g. docker build -t exdrums/hbg-identity-test -f API.Identity/Dockerfile .
