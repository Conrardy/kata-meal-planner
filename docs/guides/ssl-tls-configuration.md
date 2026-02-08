# SSL/TLS Configuration Guide

This document describes how to configure HTTPS security for MealPlanner in production environments.

## Overview

MealPlanner implements comprehensive HTTPS security measures:

- **HTTPS Redirection**: All HTTP requests are automatically redirected to HTTPS
- **HSTS (HTTP Strict Transport Security)**: Enforces HTTPS connections for 1 year
- **Secure Cookies**: All cookies marked as Secure, HttpOnly, and SameSite=Strict
- **TLS 1.2/1.3 Only**: Modern secure TLS protocols
- **SSL Certificate Support**: Production-ready certificate configuration

## Backend (.NET) Configuration

### HSTS Configuration

HSTS is automatically enabled in non-development environments with the following settings:

| Setting | Value | Description |
|---------|-------|-------------|
| `MaxAge` | 365 days (31536000 seconds) | Duration browsers cache HSTS policy |
| `IncludeSubDomains` | `true` | Apply policy to all subdomains |
| `Preload` | `true` | Eligible for browser HSTS preload list |

**Implementation**: `backend/src/Api/MealPlanner.Api/Program.cs`

```csharp
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// In middleware pipeline (non-development only)
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
app.UseHttpsRedirection();
```

### Cookie Security

All ASP.NET Identity cookies are configured with maximum security:

| Setting | Value | Description |
|---------|-------|-------------|
| `HttpOnly` | `true` | Prevents JavaScript access to cookies |
| `SecurePolicy` | `Always` | Cookies only sent over HTTPS |
| `SameSite` | `Strict` | Prevents CSRF attacks |
| `IsEssential` | `true` | Required for authentication |

**Implementation**: `backend/src/Infrastructure/MealPlanner.Infrastructure/DependencyInjection.cs`

```csharp
services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.IsEssential = true;
});
```

### HTTPS URLs Configuration

Configure the backend to listen on HTTPS in production:

**Environment Variable**:
```bash
ASPNETCORE_URLS=https://+:443;http://+:80
```

**Docker Configuration**:
```yaml
# docker-compose.yml
services:
  api:
    environment:
      - ASPNETCORE_URLS=https://+:443;http://+:80
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/app/certs/cert.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD}
    volumes:
      - ./certs:/app/certs:ro
    ports:
      - "443:443"
      - "80:80"
```

### Certificate Configuration

#### Option 1: PFX Certificate (Recommended)

Set environment variables:

```bash
ASPNETCORE_Kestrel__Certificates__Default__Path=/path/to/certificate.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password=YourCertificatePassword
```

#### Option 2: PEM Certificate

Set environment variables:

```bash
ASPNETCORE_Kestrel__Certificates__Default__Path=/path/to/certificate.crt
ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/path/to/private.key
```

#### Option 3: Development Certificate

For local development with HTTPS:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

## Frontend (Angular + Nginx) Configuration

### Development Configuration

**File**: `frontend/nginx.conf`

The default configuration includes security headers but HSTS is commented out (HTTP only):

```nginx
# Security headers
add_header X-Frame-Options "SAMEORIGIN" always;
add_header X-Content-Type-Options "nosniff" always;
add_header X-XSS-Protection "1; mode=block" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;

# HSTS disabled for development (HTTP only)
# add_header Strict-Transport-Security "max-age=31536000; includeSubDomains; preload" always;
```

### Production Configuration

**File**: `frontend/nginx.production.conf`

Full HTTPS configuration with:
- HTTP to HTTPS redirection
- TLS 1.2 and 1.3 support
- Modern cipher suites
- HSTS with preload
- OCSP stapling
- Enhanced security headers

#### Certificate Mounting

Mount SSL certificates as Docker secrets or volumes:

```yaml
# docker-compose.production.yml
services:
  frontend:
    volumes:
      - ./nginx.production.conf:/etc/nginx/conf.d/default.conf:ro
      - ./certs/cert.pem:/etc/nginx/ssl/cert.pem:ro
      - ./certs/key.pem:/etc/nginx/ssl/key.pem:ro
    ports:
      - "443:443"
      - "80:80"
```

#### Kubernetes Secret

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: mealplanner-tls
type: kubernetes.io/tls
data:
  tls.crt: <base64-encoded-cert>
  tls.key: <base64-encoded-key>
```

```yaml
apiVersion: v1
kind: Pod
spec:
  containers:
  - name: frontend
    volumeMounts:
    - name: tls-certs
      mountPath: /etc/nginx/ssl
      readOnly: true
  volumes:
  - name: tls-certs
    secret:
      secretName: mealplanner-tls
```

### TLS Configuration Details

**Supported Protocols**: TLS 1.2, TLS 1.3 (no SSLv3, TLS 1.0, TLS 1.1)

**Cipher Suites** (Modern, secure selection):
```
ECDHE-ECDSA-AES128-GCM-SHA256
ECDHE-RSA-AES128-GCM-SHA256
ECDHE-ECDSA-AES256-GCM-SHA384
ECDHE-RSA-AES256-GCM-SHA384
```

**Session Cache**: 10MB shared cache, 10-minute timeout

**OCSP Stapling**: Enabled with Google DNS resolvers (8.8.8.8, 8.8.4.4)

## Certificate Acquisition

### Let's Encrypt (Recommended for Production)

#### Using Certbot

```bash
# Install certbot
apt-get update
apt-get install certbot python3-certbot-nginx

# Obtain certificate
certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com

# Certificates location
# cert.pem: /etc/letsencrypt/live/yourdomain.com/fullchain.pem
# key.pem: /etc/letsencrypt/live/yourdomain.com/privkey.pem
```

#### Auto-renewal

```bash
# Test renewal
certbot renew --dry-run

# Setup cron job for auto-renewal
crontab -e
# Add: 0 0 * * * certbot renew --quiet --post-hook "docker restart mealplanner-frontend"
```

### Self-Signed Certificate (Development/Testing)

```bash
# Generate self-signed certificate
openssl req -x509 -newkey rsa:4096 -keyout key.pem -out cert.pem -days 365 -nodes \
  -subj "/CN=localhost/O=MealPlanner/C=US"

# Convert to PFX for .NET
openssl pkcs12 -export -out certificate.pfx -inkey key.pem -in cert.pem -password pass:YourPassword
```

## Deployment Checklist

### Backend

- [ ] HTTPS URLs configured (`ASPNETCORE_URLS`)
- [ ] SSL certificate mounted and path configured
- [ ] Certificate password stored securely (secrets/vault)
- [ ] HSTS enabled (automatic in non-development)
- [ ] HTTPS redirection enabled (automatic)
- [ ] Cookie security configured (automatic)

### Frontend

- [ ] Production nginx config used (`nginx.production.conf`)
- [ ] SSL certificate mounted (`/etc/nginx/ssl/cert.pem`)
- [ ] SSL private key mounted (`/etc/nginx/ssl/key.pem`)
- [ ] File permissions: certificates readable, key readable only by nginx user
- [ ] HSTS header enabled (automatic in production config)
- [ ] HTTP to HTTPS redirection enabled (automatic in production config)

### DNS & Network

- [ ] DNS A/AAAA records pointing to server IP
- [ ] Firewall allows ports 80 and 443
- [ ] Load balancer SSL termination configured (if applicable)
- [ ] Certificate valid for all required domains/subdomains

## Security Best Practices

1. **Certificate Storage**: Never commit certificates to version control. Use secrets management (HashiCorp Vault, AWS Secrets Manager, etc.)

2. **Rotation**: Rotate certificates before expiry. Let's Encrypt certificates expire after 90 days.

3. **HSTS Preload**: Submit domain to [HSTS Preload List](https://hstspreload.org/) after 6 months of stable HSTS deployment.

4. **Monitoring**: Monitor certificate expiry with alerts at 30 days, 14 days, and 7 days before expiration.

5. **Cipher Suites**: Regularly review and update cipher suites based on current security recommendations.

6. **TLS Version**: Disable TLS 1.0 and 1.1 (already done). Monitor for TLS 1.2 deprecation (expected 2025+).

## Testing HTTPS Configuration

### Check HTTPS Redirection

```bash
curl -I http://yourdomain.com
# Should return: HTTP/1.1 301 Moved Permanently
# Location: https://yourdomain.com
```

### Check HSTS Header

```bash
curl -I https://yourdomain.com
# Should include: Strict-Transport-Security: max-age=31536000; includeSubDomains; preload
```

### Check TLS Configuration

```bash
# Using nmap
nmap --script ssl-enum-ciphers -p 443 yourdomain.com

# Using testssl.sh
./testssl.sh https://yourdomain.com

# Using SSL Labs
# Visit: https://www.ssllabs.com/ssltest/analyze.html?d=yourdomain.com
```

### Check Cookie Security

```bash
curl -v --cookie-jar - https://yourdomain.com/api/v1/auth/login
# Look for: Secure; HttpOnly; SameSite=Strict
```

## Troubleshooting

### Backend Certificate Errors

**Error**: "Unable to configure HTTPS endpoint"

**Solution**: Check certificate path and password:
```bash
# Verify certificate exists
ls -la /path/to/certificate.pfx

# Test certificate password
openssl pkcs12 -in certificate.pfx -passin pass:YourPassword -noout
```

### Nginx Certificate Errors

**Error**: "SSL: error:0200100D:system library:fopen:Permission denied"

**Solution**: Fix file permissions:
```bash
chmod 644 /etc/nginx/ssl/cert.pem
chmod 600 /etc/nginx/ssl/key.pem
chown nginx:nginx /etc/nginx/ssl/*.pem
```

### HSTS Not Working

**Issue**: HSTS header not present in response

**Check**:
1. Ensure request is HTTPS (HSTS only applies to HTTPS)
2. Check environment is not Development (HSTS disabled in dev)
3. Verify middleware order (HSTS before HTTPS redirection)

### Mixed Content Warnings

**Issue**: Browser blocks HTTP resources on HTTPS page

**Solution**: Ensure all resources (API, images, scripts) use HTTPS or protocol-relative URLs:
```typescript
// Angular environment
export const environment = {
  apiUrl: 'https://api.yourdomain.com'  // Not http://
};
```

## References

- [OWASP Transport Layer Protection](https://cheatsheetseries.owasp.org/cheatsheets/Transport_Layer_Protection_Cheat_Sheet.html)
- [Mozilla SSL Configuration Generator](https://ssl-config.mozilla.org/)
- [HSTS Preload List](https://hstspreload.org/)
- [SSL Labs Best Practices](https://github.com/ssllabs/research/wiki/SSL-and-TLS-Deployment-Best-Practices)
- [ASP.NET Core HTTPS](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl)
