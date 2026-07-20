# 03. Hotel 영속성 설계

## 목표

- Property 모듈이 소유하는 PostgreSQL 스키마와 `DbContext` 경계를 처음으로 구현한다.
- Hotel Aggregate와 값 객체를 도메인 모델의 의미를 잃지 않고 저장하고 재구성한다.
- 운영 식별자인 Hotel Slug의 전역 유일성을 PostgreSQL 제약조건으로 보장한다.
- 이후 Room Type과 다른 모듈 영속성이 따를 수 있는 마이그레이션 및 통합 테스트 기준을 만든다.

## 적용 범위

- Property 모듈: `backend/src/HotelBooking.Modules.Property/Infrastructure/Persistence`
- API 구성: `backend/src/HotelBooking.Api`
- 최초 마이그레이션: `backend/src/HotelBooking.Modules.Property/Infrastructure/Persistence/Migrations`
- PostgreSQL 통합 테스트: `backend/tests/HotelBooking.IntegrationTests/Property/Hotels`
- 비범위: Room Type, Hotel Command/Query와 HTTP API, Outbox, 도메인 이벤트 발행, Seed, 다른 모듈의 영속성

## 사전 조건

- 로컬 또는 Testcontainers에서 PostgreSQL 17을 실행할 수 있어야 한다.
- 하나의 PostgreSQL 데이터베이스 안에서 모듈별 스키마와 `DbContext`를 사용한다.
- Hotel은 Property 모듈의 Aggregate이며 다른 모듈은 Property 영속성 타입을 직접 참조하지 않는다.
- EF Core 매핑과 마이그레이션은 PostgreSQL 통합 테스트로 검증한다.

## 단계별 적용

## 03.1 Property 영속성 경계 구성

- `PropertyDbContext`는 Property 모듈 안에 두고 `Hotel` Aggregate만 노출한다.
- 기본 스키마는 `property`를 사용한다.
- 모듈별 마이그레이션 충돌을 피하기 위해 이력 테이블은 `property.__ef_migrations_history`를 사용한다.
- 런타임 연결 문자열 이름은 Aspire 데이터베이스 리소스와 같은 `hotelbooking`을 사용한다.
- 애플리케이션 시작 시 마이그레이션을 자동 적용하지 않는다. 배포와 테스트가 명시적으로 적용한다.

## 03.2 Hotel 관계형 매핑

- `HotelId`는 애플리케이션이 생성한 PostgreSQL `uuid`로 저장하며 데이터베이스 값 생성을 사용하지 않는다.
- `HotelStatus`는 운영 가독성을 위해 문자열로 저장한다.
- `SellingCurrency`는 정규화된 세 글자 코드로 저장한다.
- Hotel Slug에는 `ux_hotels_slug` 고유 인덱스를 둔다.
- Address, GeoLocation, CheckInPolicy, HotelPolicy는 Hotel과 생명주기를 공유하는 복합 값으로 같은 행에 평탄화한다.
- ContactInfo는 모든 구성요소가 선택적이므로 값의 존재와 부재를 구분할 수 있는 nullable `jsonb` 컬럼으로 저장한다.
- 도메인 이벤트 컬렉션은 영속화하지 않는다. Outbox 변환은 후속 작업에서 구현한다.

## 03.3 최초 마이그레이션과 런타임 등록

- EF Core/Npgsql 10 패키지와 설계 시점 `DbContext` 팩터리를 추가한다.
- `InitialProperty` 마이그레이션으로 `property` 스키마, Hotel 테이블, 고유 인덱스를 생성한다.
- API는 `hotelbooking` 연결 문자열로 Property 모듈을 등록한다.
- 개발 환경 기본 연결 문자열은 루트 Compose의 PostgreSQL 설정과 일치시킨다.

## 03.4 PostgreSQL 통합 테스트

- PostgreSQL 17 Testcontainer에 프로덕션 마이그레이션을 적용한다.
- 선택 값이 없는 Draft Hotel을 저장하고 동일한 상태로 재구성하는지 검증한다.
- 모든 복합 값과 lifecycle 상태가 있는 Hotel을 저장하고 동일한 값으로 재구성하는지 검증한다.
- 중복 Slug 저장을 PostgreSQL 고유 제약조건이 거부하는지 검증한다.
- 실제 스키마와 모듈별 마이그레이션 이력 테이블이 생성되는지 검증한다.

## 스키마 규칙

- 스키마: `property`
- 테이블: `hotels`
- 기본 키: `id uuid`
- 운영 고유 키: `slug`
- 시간: `timestamp with time zone`
- Hotel 값 객체는 별도 Aggregate 테이블로 분리하지 않는다.
- 검색 조건으로 사용할 주소와 위치는 개별 컬럼으로 유지한다.

## 장애 대응/롤백

- 마이그레이션 적용 실패 시 애플리케이션 시작 과정이 스키마를 임의로 변경하지 않는다.
- 아직 운영 데이터가 없는 최초 마이그레이션이므로 롤백은 `InitialProperty` 이전 대상으로 명시적으로 되돌린다.
- Hotel 재구성 실패 시 값 객체를 우회하여 불완전한 도메인 객체를 반환하지 않는다.

## 검증 체크리스트

- [x] `property` 스키마와 모듈별 마이그레이션 이력 테이블이 생성된다.
- [x] Draft Hotel의 필수 값과 nullable 값이 PostgreSQL 왕복 후 보존된다.
- [x] 완성된 Hotel의 값 객체와 lifecycle 상태가 PostgreSQL 왕복 후 보존된다.
- [x] 중복 Hotel Slug가 PostgreSQL에서 거부된다.
- [x] Property 도메인 어셈블리가 EF Core 타입에 의존하지 않는다.
- [x] 전체 빌드, 포맷, 단위 테스트, 아키텍처 테스트가 통과한다.

검증 명령:

```bash
dotnet test backend/tests/HotelBooking.IntegrationTests/HotelBooking.IntegrationTests.csproj --filter "FullyQualifiedName~Property.Hotels"
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj
dotnet test backend/tests/HotelBooking.ArchitectureTests/HotelBooking.ArchitectureTests.csproj
dotnet build backend/HotelBooking.slnx --no-restore
dotnet format backend/HotelBooking.slnx --no-restore --verify-no-changes
dotnet ef migrations has-pending-model-changes --project backend/src/HotelBooking.Modules.Property --context PropertyDbContext --no-build
git diff --check
```

## 완료 기준(DoD)

- [x] Property 모듈이 독립된 PostgreSQL 스키마, `DbContext`, 마이그레이션 이력을 소유한다.
- [x] Hotel Aggregate의 저장과 재구성이 실제 PostgreSQL에서 증명된다.
- [x] Hotel Slug 유일성이 데이터베이스 제약조건과 통합 테스트로 증명된다.
- [x] 프로덕션 마이그레이션과 EF Core 모델 사이에 미반영 변경이 없다.
- [x] Room Type, API 유스케이스, Outbox가 이번 범위 밖임이 결과 보고에 명시된다.
