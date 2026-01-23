# Stage 1: Install dependencies
FROM node:22-alpine AS install
WORKDIR /app

# Copy package files for better caching
COPY package*.json ./

RUN npm ci --ignore-scripts

# Stage 2: Build application
FROM install AS build
ARG CONFIGURATION=production

COPY . .

RUN npm run build -- --configuration ${CONFIGURATION}

# Stage 3: Runtime image with nginx
FROM nginx:alpine AS runtime

# Remove default nginx configuration
RUN rm /etc/nginx/conf.d/default.conf

# Copy custom nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built application from build stage
COPY --from=build /app/dist/meal-planner/browser /usr/share/nginx/html

# Create non-root user for security
RUN addgroup -g 1000 appgroup && \
    adduser -u 1000 -G appgroup -D appuser && \
    chown -R appuser:appgroup /var/cache/nginx /var/run /var/log/nginx /usr/share/nginx/html && \
    touch /var/run/nginx.pid && \
    chown appuser:appgroup /var/run/nginx.pid

USER appuser

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/ || exit 1

CMD ["nginx", "-g", "daemon off;"]
