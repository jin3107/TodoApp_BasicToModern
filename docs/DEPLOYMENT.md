# Tài liệu Deployment – TodoApp_BasicToModern

Tài liệu này giải thích cách app được đóng gói, kiểm tra và phát hành bằng Docker, CI/CD và GHCR. Viết cho người đọc từ con số 0. Xem [README](../README.md) để biết cách cài đặt và dùng.

Phiên bản tài liệu khớp với trạng thái repo tại `v1.3.1`.

## 1. Mục tiêu và bối cảnh

- **Vấn đề:** chưa có VPS để host app công khai.
- **Giải pháp:** đóng gói app thành Docker image, đưa lên kho công khai (GHCR). Ai có Docker đều tải về chạy được bằng vài lệnh, giống cách n8n phân phối bản self-host.
- **Kết quả:** người dùng chỉ cần 2 file (`docker-compose.yml` và `.env`), không cần cài .NET, Node hay MySQL.

## 2. Sơ đồ tổng thể

```
 DEV (máy bạn)                GITHUB                       NGƯỜI DÙNG
 ──────────────               ──────────────               ──────────────
 sửa code
   │ git push
   ▼
 Pull Request ──────────────► CI (ci.yml)
                               • test backend
                               • build frontend
                               • thử build Docker
   │ squash merge
   ▼
 master
   │ tạo Release/tag v1.3.1
   ▼
                              CD (docker-publish.yml)
                               • build 2 image
                               • đẩy lên GHCR ───────────► docker compose pull/up
                                                           (tải image về chạy)
```

## 3. Khái niệm nền tảng

| Thuật ngữ | Giải thích | Ví dụ trong dự án |
|---|---|---|
| **Image** | Gói app đã đóng sẵn, chỉ đọc. Giống file cài đặt | `todoapp-backend`, `todoapp-frontend` |
| **Container** | Image đang chạy. Một image chạy được nhiều container | `todoapp-test-backend-1` |
| **Dockerfile** | Công thức tạo image, từng bước một | `Todo.API/Dockerfile` |
| **Volume** | Ổ lưu dữ liệu tách khỏi container. Container xóa đi thì volume còn | `mysql-data`, `redis-data` |
| **Network** | Mạng ảo để các container gọi nhau bằng tên | `todoapp-network` |
| **docker compose** | Khai báo và chạy nhiều container cùng lúc bằng 1 file | `docker-compose.yml` |
| **Registry** | Kho chứa image để người khác tải. GHCR là kho của GitHub | `ghcr.io/jin3107/...` |
| **Tag (image)** | Nhãn phiên bản của image | `1.3.1`, `latest` |
| **Tag (git)** | Nhãn đánh dấu một commit | `v1.3.1` |
| **CI** | Tự kiểm tra code mỗi lần có thay đổi | `ci.yml` |
| **CD** | Tự đóng gói và phát hành sau khi kiểm tra | `docker-publish.yml` |

**Lưu ý hay nhầm:** tag git là `v1.3.1` (có chữ `v`), còn tag image là `1.3.1` (không có `v`), vì `docker/metadata-action` tự bỏ chữ `v` khi dùng pattern semver.

## 4. Kiến trúc khi chạy (4 container)

```
 Trình duyệt ──► :8080 ──► [frontend: nginx] ──┬─► file tĩnh (React)
                                               └─► proxy ──► [backend: .NET :8080]
                                                                   │
                                                          ┌────────┴────────┐
                                                       [mysql]           [redis]
```

- **frontend** là cửa duy nhất mở ra ngoài (port 8080 của máy). Nó vừa phục vụ giao diện, vừa chuyển tiếp các request API tới backend.
- **backend** chỉ nằm trong mạng nội bộ Docker, không mở port ra ngoài.
- **mysql** và **redis** cũng nằm nội bộ. Đây là chủ ý bảo mật: DB không lộ ra mạng.

Luồng khi mở trang:

1. Trình duyệt gửi `GET /todo-lists` với `Accept: text/html`, nginx trả trang React.
2. React chạy, gọi API bằng axios (không gửi `text/html`), nginx chuyển tiếp tới backend.
3. Backend đọc/ghi MySQL, dùng Redis làm cache.

## 5. Tour từng file

### 5.1 `TodoApp.Server/src/Todo.API/Dockerfile` (backend)

Dùng **multi-stage build**:

- Giai đoạn `build`: image .NET SDK (nặng) restore, build, publish.
- Giai đoạn `final`: image ASP.NET runtime (nhẹ), chỉ copy kết quả publish sang.
- Lợi ích: image cuối nhỏ, không chứa công cụ build.

### 5.2 `TodoApp.Client/Dockerfile` (frontend)

Cũng 2 giai đoạn:

- `build`: Node 22 chạy `npm run build`, ra thư mục `dist`.
- `nginx:alpine`: copy `dist` và `nginx.conf` vào.
- Vite 7 yêu cầu Node 20.19+ hoặc 22.12+, nên Dockerfile dùng `node:22-alpine`.

### 5.3 `TodoApp.Client/nginx.conf`

Ba điểm chính:

- **SPA fallback:** `try_files $uri /index.html`. React Router xử lý đường dẫn phía trình duyệt, nên nginx phải trả `index.html` cho mọi đường dẫn không có file thật. Không có dòng này thì F5 ở `/login` sẽ 404.
- **Xử lý va chạm route:** `/todo-lists` và `/reports` vừa là trang React, vừa là prefix của API. nginx dùng `map $http_accept $wants_html`: nếu request chấp nhận `text/html` (trình duyệt mở trang) thì trả SPA, ngược lại chuyển tiếp tới backend.
- **Resolve backend lười:** `resolver 127.0.0.11` kèm `set $backend ...; proxy_pass $backend;`. Nếu viết `proxy_pass http://backend:8080` cố định thì nginx tra tên `backend` lúc khởi động, backend chưa lên là nginx chết.

### 5.4 `docker-compose.release.yml`

- **`image:`** thay vì `build:`. Compose tải image có sẵn, không cần source code.
- **`${VAR:?thông báo}`:** bắt buộc phải có biến, thiếu thì compose dừng và in thông báo, tránh chạy với mật khẩu rỗng.
- **`${VAR:-mặc_định}`:** có giá trị mặc định nếu không đặt.
- **Biến dạng `Jwt__Key`:** dấu `__` (hai gạch dưới) là cách .NET đọc cấu hình lồng. `Jwt__Key` tương ứng `Jwt:Key` trong `appsettings.json`.
- **`healthcheck` + `depends_on: condition: service_healthy`:** backend chỉ khởi động khi MySQL và Redis báo healthy.
- **`restart: unless-stopped`:** tự khởi động lại khi lỗi hoặc khi bật lại máy.
- Không có `ports:` cho mysql và redis, đây là chủ ý.

### 5.5 `.env.example`

Là mẫu các biến cần điền. Người dùng copy thành `.env`. File `.env` thật nằm trong `.gitignore`, không bao giờ commit.

| Biến | Bắt buộc | Dùng để |
|---|---|---|
| `MYSQL_ROOT_PASSWORD` | Có | Mật khẩu root MySQL |
| `MYSQL_PASSWORD` | Có | Mật khẩu user `todoapp` cho backend |
| `REDIS_PASSWORD` | Có | Mật khẩu Redis |
| `JWT_KEY` | Có (≥32 ký tự) | Khóa ký token đăng nhập |
| `BOOTSTRAP_ADMIN_EMAIL` / `_PASSWORD` | Có | Tài khoản SuperAdmin đầu tiên |
| `APP_PORT`, `TODOAPP_TAG` | Không | Cổng web, phiên bản image |
| `SMTP_*`, `RECIPIENT_EMAIL` | Không | Gửi báo cáo email định kỳ |

## 6. Phần code backend đã thêm

- **Tự migrate khi khởi động** (`Todo.API/Extensions/MigrationExtensions.cs`): gọi `Database.MigrateAsync()`, thử lại tối đa 10 lần cách nhau 3 giây. DB mới trống thì app không có bảng, và người dùng không nên phải chạy `dotnet ef`. Retry vì MySQL khởi tạo xong chậm hơn backend một chút.
- **JWT:** backend đọc `Jwt:Key`, `Jwt:Issuer`, `Jwt:Audience` để ký và kiểm tra token. Trước đây thiếu trong cấu hình production.
- **Không lộ lỗi nội bộ:** trước đây một số handler trả `ex.Message` thẳng cho client, có thể lộ chi tiết DB. Giờ trả câu chung, lỗi chi tiết ghi vào log phía server.
- **Bootstrap admin:** tài khoản admin chỉ được tạo khi đăng nhập lần đầu bằng đúng email trong `BOOTSTRAP_ADMIN_EMAIL`. Đổi email trong `.env` sau đó không xóa tài khoản cũ.

## 7. CI/CD chi tiết

### 7.1 `.github/workflows/ci.yml`

Chạy khi mở PR vào `master` và khi push lên `master`:

- **backend:** `dotnet test`.
- **frontend:** `npm ci` rồi `npm run build` trên Node 22.
- **docker-build:** build cả hai Dockerfile nhưng không đẩy. Mục đích: bắt lỗi Dockerfile trước khi phát hành.

### 7.2 `.github/workflows/docker-publish.yml`

Chạy khi push tag `v*` hoặc chạy tay:

1. Checkout code tại tag.
2. Đăng nhập GHCR bằng `GITHUB_TOKEN` (GitHub tự cấp, không cần tạo secret).
3. Build và đẩy mỗi image với hai tag: `1.3.1` và `latest`.
4. Dùng cache của GitHub Actions để lần sau build nhanh hơn.

### 7.3 Giới hạn của CI hiện tại

CI chỉ chứng minh **image build được**, chưa chứng minh **image chạy đúng**. Đây là lý do lỗi thiếu `nginx.conf` ở bản `1.3.0` lọt qua, và chỉ lộ ra khi chạy thử trên máy thật.

## 8. Quy trình phát hành (checklist)

1. Làm trên một branch riêng, commit, push.
2. Mở Pull Request vào `master`. Chờ 3 job CI xanh.
3. **Squash and merge.**
4. Trên GitHub: Releases → Draft a new release → tag mới (ví dụ `v1.3.2`), target `master`, viết ghi chú → Publish.
5. Tab Actions: chờ "Publish Docker images" xanh (khoảng 2–3 phút).
6. Lần đầu: Packages → mỗi package → Package settings → Change visibility → **Public**.
7. **Test như người dùng thật** (mục 9).
8. Nếu bản đã phát hành bị lỗi: **không ghi đè tag cũ**, sửa rồi phát hành bản vá mới (cách `v1.3.1` thay `v1.3.0`).

Đánh số phiên bản `MAJOR.MINOR.PATCH`: sửa lỗi nhỏ tăng PATCH, thêm tính năng tăng MINOR, thay đổi phá vỡ tương thích tăng MAJOR.

## 9. Cách test một bản phát hành

Dùng thư mục trống, tách khỏi repo (WSL: `~/todoapp-test`). Lưu file compose với tên `docker-compose.yml` để không cần `-f`:

```bash
curl -o docker-compose.yml https://raw.githubusercontent.com/jin3107/TodoApp_BasicToModern/master/docker-compose.release.yml
curl -o .env https://raw.githubusercontent.com/jin3107/TodoApp_BasicToModern/master/.env.example
# điền .env
docker compose up -d
```

Kiểm tra theo thứ tự:

1. `docker compose ps`: cả 4 container Up, mysql và redis healthy.
2. Log backend có dòng `Database migrations applied`.
3. Các route trả 200 khi gửi `Accept: text/html`: `/login`, `/dashboard`, `/todo-lists`, `/reports`.
4. Trên trình duyệt: đăng nhập admin, F5 ở `/todo-lists`, tạo list và todo.

**Bẫy khi test:** nếu bạn từng build image tay và gắn cùng tên với image công khai, Docker dùng bản trên máy thay vì tải từ GHCR. Cần `docker rmi` hoặc dùng tag phiên bản cụ thể để chắc chắn đang test bản đã phát hành.

## 10. Vận hành hằng ngày

Các lệnh dưới giả định file tên `docker-compose.yml` và chạy trong thư mục có `.env`.

| Việc | Lệnh |
|---|---|
| Chạy | `docker compose up -d` |
| Xem trạng thái | `docker compose ps` |
| Xem log backend | `docker compose logs --tail=80 backend` |
| Dừng, giữ dữ liệu | `docker compose down` |
| Dừng và **xóa dữ liệu** | `docker compose down -v` |
| Cập nhật bản mới | `docker compose pull` rồi `docker compose up -d` |
| Vào MySQL | `docker compose exec mysql sh -c 'mysql -uroot -p"$MYSQL_ROOT_PASSWORD"'` |

- **`down` và `down -v` khác nhau:** `down` xóa container nhưng giữ volume (dữ liệu còn). `down -v` xóa cả volume, mất sạch dữ liệu. Chỉ dùng `-v` khi muốn làm lại từ đầu.
### Cập nhật đúng cách và dọn image cũ

- **`up -d` không tự tải bản mới.** Nếu máy đã có tag `latest` thì Compose dùng luôn bản đó. Muốn lấy bản mới phải `docker compose pull` trước, rồi `docker compose up -d`.
- Sau khi cập nhật, các tag cũ (ví dụ `1.3.0`) vẫn nằm trên máy và chiếm dung lượng. Xem và dọn:

```bash
docker images                               # xem toàn bộ image
docker rmi ghcr.io/jin3107/todoapp-frontend:1.3.0   # xóa một tag cụ thể
docker image prune                          # chỉ xóa image "dangling" (không còn tag nào)
docker system df                            # xem Docker đang chiếm bao nhiêu dung lượng
```

- **Cẩn thận với `docker image prune -a`:** nó xóa **mọi** image không có container nào đang dùng, kể cả image bạn cần cho project khác (ví dụ `mysql`, `redis`, image học tập). Chỉ dùng khi chắc chắn, và kiểm tra `docker images` trước.
- Image tự build để thử (ví dụ `todoapp-frontend-test`) nên xóa sau khi xong, và không gắn trùng tên với image công khai để tránh lẫn lộn khi test (mục 9).

### Lưu ý về mật khẩu MySQL

- **Mật khẩu MySQL chỉ có hiệu lực ở lần khởi tạo volume đầu tiên.** Đổi `MYSQL_ROOT_PASSWORD` trong `.env` sau đó không đổi mật khẩu trong DB. Muốn áp dụng thì `down -v` (mất dữ liệu) hoặc đổi trực tiếp bằng SQL.

## 11. Xem dữ liệu bằng DBeaver

Mặc định MySQL không mở port, nên cần file override tạm `docker-compose.dbeaver.yml` (không đưa vào repo):

```yaml
services:
  mysql:
    ports:
      - "127.0.0.1:3308:3306"
```

Chạy: `docker compose -f docker-compose.yml -f docker-compose.dbeaver.yml up -d`.

- **Cú pháp port `HOST:CONTAINER`:** số bên phải (3306) là port MySQL bên trong container, luôn không đổi. Số bên trái (3308) là port trên máy bạn, phải chưa bị ai dùng.
- Cấu hình DBeaver: Host `localhost`, Port `3308`, user `root` (hoặc `todoapp`, chỉ có quyền trên một DB), mật khẩu theo `.env`.
- **`Server Host` phải là địa chỉ phân giải được từ Windows** (`localhost`, `127.0.0.1`). Tên container hay tên tùy ý không dùng được, vì DNS của Docker chỉ có tác dụng bên trong mạng Docker. Tên kết nối đặt ở tab General.

## 12. Bảo mật

- **Không commit `.env`.** Đã chặn trong `.gitignore`.
- **Không đưa DB ra mạng.** Bản release cố ý không publish port MySQL và Redis. Override cho DBeaver chỉ bind `127.0.0.1`.
- **Không có mật khẩu mặc định yếu** trong bản release; thiếu biến thì compose từ chối chạy.
- **Lộ secret:** nếu mật khẩu, app password Gmail hay JWT key xuất hiện trong ảnh chụp, log hay chat thì coi như đã lộ. Thu hồi và tạo lại, không chỉ xóa ảnh.
- **`JWT_KEY`:** đổi key sẽ làm mọi token đang đăng nhập mất hiệu lực. Không dùng chung key giữa các môi trường.
- **Cổng 8080 nếu mở ra internet:** hiện chạy HTTP không mã hóa. Dùng thật cần HTTPS qua reverse proxy (mục 15).

## 13. Sự cố đã gặp và bài học

| Sự cố | Nguyên nhân | Bài học |
|---|---|---|
| `crypto.hash is not a function` khi build frontend | Dockerfile dùng Node 18, Vite 7 cần Node ≥ 20.19 | Kiểm tra yêu cầu phiên bản runtime của công cụ build |
| Vào `/login` bị **404** ở bản `1.3.0` | Dockerfile không copy `nginx.conf` vào image, nginx dùng cấu hình mặc định | CI chỉ kiểm tra "build được", cần test "chạy đúng". Bản vá: `1.3.1` |
| 3 dòng lỗi kết nối DB ở lần chạy đầu | MySQL khởi tạo bằng server tạm; healthcheck qua socket (`localhost`) báo healthy sớm | Healthcheck dùng `-h 127.0.0.1` (TCP); retry là lớp bảo vệ thứ hai |
| DBeaver báo `Access denied` dù mật khẩu đúng | Port 3307 bị MySQL của XAMPP chiếm, DBeaver nối nhầm | Khi lỗi lạ, kiểm tra **ai đang nghe port** (`netstat -ano \| findstr :PORT`) |
| `Unknown host` trong DBeaver | Ô Server Host bị gõ tên tùy ý thay vì `localhost` | Host là địa chỉ mạng, không phải nhãn |
| Test ra bản cũ dù đã phát hành bản mới | Image build tay trùng tên với image công khai | Dùng tag phiên bản cụ thể hoặc xóa image local |
| Đổi email admin trong `.env` mà vẫn thấy tài khoản cũ | Bootstrap admin chỉ tạo mới, không xóa cái cũ | Dữ liệu trong volume độc lập với `.env` |

## 14. Điều chưa kiểm chứng

- Luồng đăng nhập và dùng app trên trình duyệt với image `1.3.1` công khai: đã `curl` kiểm tra route, chưa xác nhận đầy đủ thao tác thật.
- Serilog ghi file vào `/app/logs`: container chạy bằng user không phải root nên có thể bị từ chối quyền ghi file; log ra console vẫn hoạt động.
- Gửi email báo cáo qua Gmail trong container.
- Chưa có test tự động chạy cả stack (smoke test) trong CI.

## 15. Hướng phát triển tiếp theo

- **Smoke test trong CI:** dựng stack bằng compose rồi `curl` các route, để bắt lỗi kiểu thiếu `nginx.conf` ngay ở PR.
- **ESLint trong CI:** chưa bật vì chưa biết code hiện tại có sạch lint không.
- **HTTPS và tên miền:** đặt reverse proxy (Caddy, Traefik hoặc nginx) phía trước khi có VPS.
- **Sao lưu:** lịch `mysqldump` ra file, vì dữ liệu chỉ nằm trong một volume.
- **Branch protection:** bắt buộc CI xanh mới được merge vào `master`.
- **Dọn branch cũ:** `feature/auth`, `feature/handler-pattern`, `fix/gmail-otp` đã lỗi thời.

## 16. Từ điển nhanh

- **Squash merge:** gộp mọi commit của PR thành một commit khi vào `master`.
- **Multi-stage build:** dùng nhiều giai đoạn trong một Dockerfile, chỉ giữ kết quả cuối.
- **SPA (Single Page Application):** app chạy trọn trong một trang HTML, đường dẫn do JavaScript xử lý.
- **Reverse proxy:** máy chủ đứng trước, nhận request rồi chuyển cho dịch vụ phía sau.
- **Healthcheck:** lệnh Docker chạy định kỳ để biết dịch vụ có thật sự sẵn sàng.
- **Migration:** kịch bản thay đổi cấu trúc DB theo phiên bản.
- **Bootstrap:** bước khởi tạo ban đầu (ở đây là tạo admin đầu tiên).
