FROM node:22.12-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build

FROM node:22.12-alpine
WORKDIR /app
RUN npm install -g serve
COPY --from=build /app/dist/AtlasPortfolioEngine.Web/browser .
CMD ["sh", "-c", "serve -s . -l ${PORT:-3000}"]
