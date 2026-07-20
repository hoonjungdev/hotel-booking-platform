# 04. Room Type 영속성 설계

## 목표

- Property 모듈의 `RoomType` Aggregate를 기존 `property` PostgreSQL 경계 안에 저장하고 재구성한다.
- 호텔별 운영 식별자인 Room Type Code의 유일성과 Hotel 참조 무결성을 데이터베이스 제약조건으로 보장한다.
- `Occupancy`와 `BedComposition` 값 객체를 도메인 의미와 Aggregate 생명주기를 유지하며 매핑한다.
- 이후 Guest Hotel Search, Inventory, Pricing이 사용할 안정적인 Room Type 영속성 기반을 만든다.

## 적용 범위

- Property 모듈: `backend/src/HotelBooking.Modules.Property/Infrastructure/Persistence`
- Property 마이그레이션: `backend/src/HotelBooking.Modules.Property/Infrastructure/Persistence/Migrations`
- PostgreSQL 통합 테스트: `backend/tests/HotelBooking.IntegrationTests/Property/RoomTypes`
- 기존 Property PostgreSQL 테스트 픽스처의 Room Type 테이블 초기화 지원
- 비범위: Room Type Command/Query와 HTTP API, Hotel Aggregate에 Room Type 컬렉션 추가, 객실 실물(Room) 영속성, Guest Hotel Search, Inventory/Pricing 연결, Seed, Outbox

## 사전 조건

- `03. Hotel 영속성 설계`의 `PropertyDbContext`, `property` 스키마, 모듈별 마이그레이션 이력이 적용되어 있어야 한다.
- Room Type은 Hotel과 별도 Aggregate Root이며 `HotelId`만 참조한다.
- Room Type Code는 도메인에서 공백을 제거하고 대문자로 정규화한다.
- 통합 테스트는 PostgreSQL 17 Testcontainer에 프로덕션 마이그레이션을 적용한다.

## 단계별 적용

## 04.1 Room Type 관계형 매핑

- `PropertyDbContext`에 `RoomType` Aggregate 집합을 추가한다.
- `RoomTypeId`와 `HotelId`는 PostgreSQL `uuid`로 변환하며 데이터베이스 값 생성을 사용하지 않는다.
- `RoomTypeStatus`와 `BedType`은 운영 가독성을 위해 문자열로 저장한다.
- `RoomTypeCode`는 정규화된 최대 30자 문자열로 저장한다.
- `Occupancy`는 Room Type과 생명주기가 같고 독립 조회 대상이 아니므로 `room_types` 행에 평탄화한다.
- Occupancy의 양수·음수 경계와 세 최대값의 상호 일관성은 `ck_room_types_occupancy_valid` CHECK 제약으로도 보호한다.
- 도메인 이벤트 컬렉션과 계산 속성 `IsSellable`은 영속화하지 않는다.

## 04.2 Aggregate 관계와 데이터베이스 제약조건

- Hotel과 Room Type은 별도 Aggregate이므로 `RoomType.HotelId` 외래 키만 두고 도메인 탐색 속성을 추가하지 않는다.
- Hotel 삭제는 별도 유스케이스와 정책이 정의되지 않았으므로 Room Type이 존재하면 `RESTRICT`한다.
- `(hotel_id, code)`에 `ux_room_types_hotel_id_code` 고유 인덱스를 두어 같은 호텔 안의 코드 중복을 거부한다.
- 같은 Room Type Code는 서로 다른 호텔에서 사용할 수 있다.
- PostgreSQL B-tree의 선두 컬럼 탐색을 활용하므로 `hotel_id` 단독 인덱스를 중복 생성하지 않는다.

## 04.3 Bed Composition 소유 컬렉션 매핑

- `BedComposition`은 독립 Aggregate나 Entity가 아닌 Room Type 소유 값 컬렉션이다.
- 값 컬렉션은 `room_type_bed_compositions` 테이블에 저장하고 각 행은 영속성 전용 자동 증가 식별자를 사용한다.
- 각 행은 Room Type 외래 키, Bed Type, Quantity를 보존한다.
- Quantity의 양수 불변식은 `ck_room_type_bed_compositions_quantity_positive` CHECK 제약으로도 보호한다.
- Room Type이 삭제되면 소유 값은 함께 삭제되도록 `CASCADE`한다.
- 동일한 Bed Type을 여러 항목으로 표현하는 현재 도메인 입력은 새 영속성 규칙으로 금지하지 않는다. 합산 또는 중복 금지가 필요하면 별도 도메인 변경으로 다룬다.

## 04.4 Property 마이그레이션

- 기존 `InitialProperty` 다음 마이그레이션으로 `room_types`와 `room_type_bed_compositions`를 생성한다.
- 기존 Hotel 테이블과 마이그레이션을 다시 작성하지 않는다.
- Down 마이그레이션은 소유 값 테이블을 먼저 제거한 뒤 Room Type 테이블을 제거한다.

## 04.5 PostgreSQL 통합 테스트

- 여러 Bed Composition을 가진 Draft Room Type의 필수 값과 값 객체가 왕복 후 보존되는지 검증한다.
- lifecycle 상태를 변경한 Room Type이 같은 상태로 재구성되는지 검증한다.
- 존재하지 않는 Hotel을 참조하는 Room Type을 외래 키가 거부하는지 검증한다.
- 같은 호텔의 중복 Room Type Code를 고유 제약조건이 거부하는지 검증한다.
- 다른 호텔에서는 같은 Room Type Code를 허용하는지 검증한다.
- 유효하지 않은 Occupancy와 Bed Composition Quantity를 CHECK 제약조건이 거부하는지 검증한다.
- 프로덕션 마이그레이션이 Room Type 테이블, 소유 값 테이블, 외래 키와 고유 인덱스를 생성하는지 검증한다.

## 스키마 규칙

- 스키마: `property`
- Aggregate 테이블: `room_types`
- 소유 값 테이블: `room_type_bed_compositions`
- Room Type 기본 키: `id uuid`
- Hotel 참조: `hotel_id uuid`, 삭제 `RESTRICT`
- 운영 고유 키: `(hotel_id, code)`
- Occupancy 컬럼: `max_adults`, `max_children`, `max_occupancy`
- Occupancy CHECK: 도메인의 양수·음수 경계와 세 최대값의 상호 일관성
- 시간: `timestamp with time zone`
- Bed Composition 기본 키는 도메인에 노출되지 않는 `bigint` 영속성 식별자다.
- Bed Composition의 Room Type 참조는 삭제 `CASCADE`다.
- Bed Composition Quantity는 1 이상이어야 한다.

## 장애 대응/롤백

- 외래 키나 고유 제약조건 위반 시 해당 `SaveChanges` 트랜잭션 전체가 실패하며 불완전한 Room Type을 저장하지 않는다.
- 정상 쓰기는 도메인 팩토리와 Aggregate 동작을 통해서만 수행하며, EF 재구성은 마이그레이션된 스키마의 데이터를 신뢰한다.
- 데이터베이스 CHECK 제약은 Occupancy와 각 Bed Composition의 스칼라 불변식을 방어한다.
- 별도 소유 테이블의 최소 한 행 규칙은 단순 CHECK로 표현할 수 없으므로 외부 SQL로 Aggregate 내부 행을 직접 변경하는 경로는 지원하지 않는다.
- 아직 운영 데이터가 없는 마이그레이션이므로 롤백은 `AddRoomTypePersistence` 이전 대상으로 명시적으로 되돌린다.
- 기존 Hotel 데이터와 테이블은 Down 마이그레이션에서도 보존한다.

## 검증 체크리스트

- [x] Room Type 필수 값과 lifecycle 상태가 PostgreSQL 왕복 후 보존된다.
- [x] Occupancy와 여러 Bed Composition이 PostgreSQL 왕복 후 보존된다.
- [x] 존재하지 않는 Hotel 참조가 PostgreSQL에서 거부된다.
- [x] 같은 호텔의 중복 Room Type Code가 거부되고 다른 호텔의 같은 코드는 허용된다.
- [x] 유효하지 않은 Occupancy와 Bed Composition Quantity가 PostgreSQL에서 거부된다.
- [x] Room Type과 Bed Composition 테이블 및 제약조건이 프로덕션 마이그레이션으로 생성된다.
- [x] Property 도메인 어셈블리가 EF Core 타입에 의존하지 않는다.
- [x] 전체 빌드, 포맷, 단위 테스트, 통합 테스트, 아키텍처 테스트가 통과한다.

검증 명령:

```bash
dotnet test backend/tests/HotelBooking.IntegrationTests/HotelBooking.IntegrationTests.csproj --filter "FullyQualifiedName~Property.RoomTypes"
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj
dotnet test backend/tests/HotelBooking.ArchitectureTests/HotelBooking.ArchitectureTests.csproj
dotnet test backend/HotelBooking.slnx --no-restore
dotnet build backend/HotelBooking.slnx --no-restore
dotnet format backend/HotelBooking.slnx --no-restore --verify-no-changes
dotnet ef migrations has-pending-model-changes --project backend/src/HotelBooking.Modules.Property --context PropertyDbContext --no-build
git diff --check
```

## 완료 기준(DoD)

- [x] Property 모듈이 Hotel과 분리된 Room Type Aggregate 영속성을 소유한다.
- [x] Hotel 참조와 호텔별 Room Type Code 유일성이 PostgreSQL 제약조건과 통합 테스트로 증명된다.
- [x] Occupancy와 Bed Composition이 도메인 의미와 생명주기를 유지하며 재구성된다.
- [x] 프로덕션 마이그레이션과 EF Core 모델 사이에 미반영 변경이 없다.
- [x] API, 검색, Inventory/Pricing 연결과 Outbox가 이번 범위 밖임이 결과 보고에 명시된다.
