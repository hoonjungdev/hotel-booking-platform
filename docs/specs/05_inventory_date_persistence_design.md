# 05. Inventory Date 영속성 설계

## 목표

- Inventory 모듈이 소유하는 PostgreSQL 스키마와 `DbContext` 경계를 처음으로 구현한다.
- 호텔, 객실 유형, 투숙일별 `InventoryDate` Aggregate와 수량 상태를 저장하고 재구성한다.
- 동일한 재고 단위의 중복과 수량 불변식 위반을 PostgreSQL 제약조건으로 방어한다.
- 후속 원자적 다박 Inventory Hold와 동시성 제어가 사용할 안정적인 영속성 기반을 만든다.

## 적용 범위

- Inventory 모듈: `backend/src/HotelBooking.Modules.Inventory/Infrastructure/Persistence`
- API 구성: `backend/src/HotelBooking.Api`
- 최초 Inventory 마이그레이션: `backend/src/HotelBooking.Modules.Inventory/Infrastructure/Persistence/Migrations`
- PostgreSQL 통합 테스트: `backend/tests/HotelBooking.IntegrationTests/Inventory/InventoryDates`
- 비범위: `InventoryHold` 영속성, 원자적 다박 재고 확보 유스케이스, 동시성 제어와 경쟁 테스트, Inventory Command/Query와 HTTP API, Guest Hotel Search, Seed, Outbox/Inbox, Booking Saga, Payment

## 사전 조건

- `04. Room Type 영속성 설계`까지 완료되어 Hotel과 Room Type이 Property 모듈에 저장되어 있어야 한다.
- 하나의 PostgreSQL 데이터베이스 안에서 모듈별 스키마와 `DbContext`를 사용한다.
- Inventory 모듈은 Property 모듈 구현을 참조하지 않고 `HotelId`, `RoomTypeId` 참조 식별자만 보관한다.
- EF Core 매핑과 마이그레이션은 PostgreSQL 17 Testcontainer에 프로덕션 마이그레이션을 적용하여 검증한다.

## 단계별 적용

## 05.1 Inventory 영속성 경계 구성

- `InventoryDbContext`는 Inventory 모듈 안에 두고 이번 단계에서는 `InventoryDate` Aggregate만 노출한다.
- 기본 스키마는 `inventory`를 사용한다.
- 모듈별 마이그레이션 충돌을 피하기 위해 이력 테이블은 `inventory.__ef_migrations_history`를 사용한다.
- 런타임 연결 문자열 이름은 Aspire 데이터베이스 리소스와 같은 `hotelbooking`을 사용한다.
- 애플리케이션 시작 시 마이그레이션을 자동 적용하지 않는다. 배포와 테스트가 명시적으로 적용한다.

## 05.2 Inventory Date 관계형 매핑

- `InventoryDateId`, `HotelId`, `RoomTypeId`는 PostgreSQL `uuid`로 저장하고 데이터베이스 값 생성을 사용하지 않는다.
- `OccupiedDate`는 시간대가 없는 PostgreSQL `date`로 저장한다.
- `TotalQuantity`, `HeldQuantity`, `BookedQuantity`, `ClosedQuantity`는 필수 정수 컬럼으로 저장한다.
- 계산 속성 `AvailableQuantity`와 도메인 이벤트 컬렉션은 영속화하지 않는다.
- `(hotel_id, room_type_id, occupied_date)`에 `ux_inventory_dates_hotel_id_room_type_id_occupied_date` 고유 인덱스를 둔다.
- 개별 수량이 음수가 아닌지 `ck_inventory_dates_quantities_non_negative` CHECK 제약으로 보호한다.
- `held_quantity + booked_quantity + closed_quantity <= total_quantity` 규칙을 `ck_inventory_dates_committed_quantity_within_total` CHECK 제약으로 보호한다.

## 05.3 모듈 경계와 참조 무결성

- Inventory 모듈은 Property 모듈의 테이블이나 EF Core 타입을 직접 참조하지 않는다.
- `hotel_id`와 `room_type_id`에는 `property` 스키마를 향하는 데이터베이스 외래 키를 두지 않는다.
- Hotel과 Room Type 존재 여부 및 서로의 소속 관계는 후속 Inventory 작성 유스케이스의 계약 또는 통합 이벤트 경계에서 검증한다.
- 이 선택은 한 모듈의 스키마 변경과 마이그레이션이 다른 모듈의 내부 테이블 구조에 결합되는 것을 방지한다.

## 05.4 최초 Inventory 마이그레이션과 런타임 등록

- EF Core/Npgsql 10 패키지와 설계 시점 `DbContext` 팩터리를 Inventory 모듈에 추가한다.
- `InitialInventory` 마이그레이션으로 `inventory` 스키마, `inventory_dates` 테이블, 고유 인덱스와 CHECK 제약조건을 생성한다.
- API는 `hotelbooking` 연결 문자열로 Inventory 모듈을 등록한다.
- Property와 Inventory는 같은 데이터베이스를 사용하지만 서로 다른 `DbContext`, 기본 스키마, 마이그레이션 이력 테이블을 유지한다.

## 05.5 PostgreSQL 통합 테스트

- 초기 수량만 가진 Inventory Date를 저장하고 동일한 값과 계산된 Available Quantity로 재구성하는지 검증한다.
- Held, Booked, Closed Quantity가 변경된 Inventory Date를 저장하고 상태가 보존되는지 검증한다.
- 동일한 Hotel, Room Type, Occupied Date 조합의 중복 저장을 PostgreSQL 고유 제약조건이 거부하는지 검증한다.
- 서로 다른 Hotel, Room Type 또는 Occupied Date 조합은 허용하는지 검증한다.
- 음수 수량과 전체 수량을 초과하는 합계를 CHECK 제약조건이 거부하는지 검증한다.
- 실제 스키마, 테이블, 인덱스, CHECK 제약조건과 모듈별 마이그레이션 이력 테이블이 생성되는지 검증한다.

## 스키마 규칙

- 스키마: `inventory`
- 테이블: `inventory_dates`
- 기본 키: `id uuid`
- 재고 단위 고유 키: `(hotel_id, room_type_id, occupied_date)`
- 투숙일: `occupied_date date`
- 수량 컬럼: `total_quantity`, `held_quantity`, `booked_quantity`, `closed_quantity`
- 모든 수량은 0 이상이어야 한다.
- Held, Booked, Closed Quantity의 합은 Total Quantity를 초과할 수 없다.
- `AvailableQuantity`는 저장하지 않고 도메인 모델에서 계산한다.
- Property 모듈 스키마로 향하는 외래 키를 두지 않는다.

## 장애 대응/롤백

- 고유 또는 CHECK 제약조건 위반 시 해당 `SaveChanges` 트랜잭션 전체가 실패하며 불완전한 Inventory Date를 저장하지 않는다.
- 정상 쓰기는 도메인 팩토리와 Aggregate 동작을 통해 수행하며, EF 재구성은 마이그레이션된 스키마의 데이터를 신뢰한다.
- 마이그레이션 적용 실패 시 애플리케이션 시작 과정이 스키마를 임의로 변경하지 않는다.
- 아직 운영 데이터가 없는 최초 마이그레이션이므로 롤백은 `InitialInventory` 이전 대상으로 명시적으로 되돌린다.
- Property 스키마와 데이터는 Inventory 마이그레이션과 롤백의 영향을 받지 않는다.

## 검증 체크리스트

- [x] `inventory` 스키마와 모듈별 마이그레이션 이력 테이블이 생성된다.
- [x] 초기 Inventory Date의 필수 값과 계산 결과가 PostgreSQL 왕복 후 보존된다.
- [x] Held, Booked, Closed Quantity 상태가 PostgreSQL 왕복 후 보존된다.
- [x] 동일한 재고 단위의 중복이 PostgreSQL에서 거부된다.
- [x] 음수 수량과 Total Quantity를 초과하는 수량 합계가 PostgreSQL에서 거부된다.
- [x] Inventory 모듈이 Property 모듈 구현이나 스키마에 의존하지 않는다.
- [x] 전체 빌드, 포맷, 단위 테스트, 통합 테스트, 아키텍처 테스트가 통과한다.

검증 명령:

```bash
dotnet test backend/tests/HotelBooking.IntegrationTests/HotelBooking.IntegrationTests.csproj --filter "FullyQualifiedName~Inventory.InventoryDates"
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj
dotnet test backend/tests/HotelBooking.ArchitectureTests/HotelBooking.ArchitectureTests.csproj
dotnet test backend/HotelBooking.slnx --no-restore
dotnet build backend/HotelBooking.slnx --no-restore
dotnet format backend/HotelBooking.slnx --no-restore --verify-no-changes
dotnet ef migrations has-pending-model-changes --project backend/src/HotelBooking.Modules.Inventory --context InventoryDbContext --no-build
git diff --check
```

## 완료 기준(DoD)

- [x] Inventory 모듈이 독립된 PostgreSQL 스키마, `DbContext`, 마이그레이션 이력을 소유한다.
- [x] Inventory Date의 저장과 재구성이 실제 PostgreSQL에서 증명된다.
- [x] 재고 단위 유일성과 수량 불변식이 데이터베이스 제약조건과 통합 테스트로 증명된다.
- [x] 프로덕션 마이그레이션과 EF Core 모델 사이에 미반영 변경이 없다.
- [x] Inventory Hold, 동시성 제어, API, 검색, Seed, Outbox와 Saga가 이번 범위 밖임이 결과 보고에 명시된다.
