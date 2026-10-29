# ==========================================
# GIAI ĐOẠN 1: BUILD ANGULAR (Node.js)
# ==========================================
FROM node:22-alpine AS build
WORKDIR /app

# Cache package dependencies
COPY package*.json ./
RUN npm ci

# Copy toàn bộ src code và build production
COPY . ./
RUN npm run build -- --configuration production

# ==========================================
# GIAI ĐOẠN 2: RUNTIME NGINX (Cực nhẹ ~25MB)
# ==========================================
FROM nginx:alpine AS runtime

# Copy file cấu hình Nginx tự viết 
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy thư mục dist sau khi build vào thư mục web của Nginx
# Lưu ý: Angular 17/18 dùng Application builder sẽ xuất ra thư mục dist/blog-app/browser
COPY --from=build /app/dist/blog-app/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]