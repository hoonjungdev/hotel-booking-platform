# 02. Reservation 도메인 설계

## 목표

- 유효한 가격 합의와 인증된 Guest를 하나의 `Pending` Reservation으로 보존한다.
- Pricing 구현 타입에 의존하지 않는 Booking 소유의 `Reservation Price Snapshot`을 만든다.
- 재고 확보 결과에 따른 첫 Reservation 상태 전이를 Aggregate 안에서 보호한다.
- 이후 Outbox, Booking Saga, Payment 흐름이 연결될 안정적인 도메인 기반을 마련한다.

## 적용 범위

- 도메인 용어: `CONTEXT.md`
- Booking 도메인: `backend/src/HotelBooking.Modules.Booking/Domain/Reservations`
- Booking 참조 식별자: `backend/src/HotelBooking.Modules.Booking/Domain/References`
- 단위 테스트: `backend/tests/HotelBooking.UnitTests/Booking`
- 비범위: Identity의 Guest Aggregate, 비회원 예약, Guest 연락처 스냅샷, EF Core 매핑, PostgreSQL 마이그레이션, PriceQuote 조회, 애플리케이션 명령, HTTP API, Outbox, Saga, Payment, 취소, 만료, 확정

## 사전 조건

- 첫 버전 Reservation은 Identity가 소유할 인증된 Guest의 식별자를 필수로 참조한다.
- `StayDateRange`는 체크인을 포함하고 체크아웃을 제외한 점유일을 제공한다.
- `Money`는 같은 통화끼리만 합산할 수 있다.
- Pricing의 PriceQuote는 투숙 기간의 날짜별 가격과 취소 조건을 이미 검증한다.
- 첫 번째 예약 흐름은 객실 한 개만 대상으로 한다.

## 단계별 적용

## 02.1 Booking 소유 식별자와 요청 인원 구현

- `ReservationId`는 Booking이 생성하고 소유하는 `Guid` 기반 강한 타입 식별자다.
- `GuestId`, `HotelId`, `RoomTypeId`, `RatePlanId`, `PriceQuoteId`는 각 소유 모듈의 식별자를 Booking 안에서 참조한다.
- Booking의 `RequestedOccupancy`는 한 객실에 투숙할 성인과 어린이 수를 보존한다.
- Requested Occupancy는 성인 한 명 이상과 음수가 아닌 어린이 수를 요구한다.

## 02.2 Reservation Price Snapshot 구현

- 스냅샷은 Price Quote, Hotel, Room Type, Rate Plan 식별자와 투숙 기간, 요청 인원을 보존한다.
- 점유일별 합의 가격은 날짜순으로 보존하며 모든 점유일을 정확히 한 번 포함해야 한다.
- 모든 날짜별 가격은 같은 통화를 사용해야 하고 총액은 Aggregate 입력으로 받지 않고 내부에서 합산한다.
- 취소 조건은 고유한 사전 통지 경계와 0시간 fallback 규칙을 포함한 Booking 소유의 불변 값으로 복사한다.
- 같은 식별자, 투숙 조건, 날짜별 가격과 취소 조건을 가진 스냅샷은 구조적으로 같은 값이다.
- Pricing의 Aggregate나 Value Object를 Booking 도메인 API에 노출하지 않는다.

## 02.3 Reservation Aggregate와 첫 상태 전이 구현

- Reservation 생성 인터페이스는 식별자, Guest 참조, 가격 스냅샷, 명시적 생성 시각을 받는다.
- 새 Reservation은 항상 `Pending`으로 시작한다.
- 재고 확보 성공은 `Pending` Reservation을 `AwaitingPayment`로 전이한다.
- 재고 확보 실패는 `Pending` Reservation을 `Failed`로 전이한다.
- 재고 결과는 생성 시각보다 이를 수 없으며 이미 결과가 결정된 Reservation은 다시 전이할 수 없다.
- 이번 슬라이스의 `ReservationStatus`는 `Pending`, `AwaitingPayment`, `Failed`만 공개한다.
- `Confirmed`, `Cancelled`, `Expired` 상태와 그 동작은 Payment 이후 슬라이스에서 구현한다.

## 02.4 단위 테스트 작성

- 정상적인 다박 가격 스냅샷이 날짜순 가격, 총액, 식별자, 요청 인원과 취소 조건을 보존하는지 검증한다.
- 누락·중복·기간 밖 점유일과 다른 통화 가격을 거부하는지 검증한다.
- 취소 조건의 누락된 fallback, 중복 경계, 잘못된 penalty 값을 거부하는지 검증한다.
- 별도 인스턴스로 재구성한 같은 가격 스냅샷과 취소 조건이 같은 값과 해시를 갖는지 검증한다.
- Reservation이 `Pending`으로 시작하고 재고 결과에 따라 올바르게 전이하는지 검증한다.
- 누락된 식별자와 시각, 과거 결과 시각, 중복 및 역방향 상태 전이를 거부하는지 검증한다.

## 도메인 규칙

- Reservation은 한 Guest가 한 Room Type의 객실 한 개를 예약하려는 lifecycle record다.
- Reservation은 가격 견적 원본을 참조하면서도 합의된 값은 자신의 `Reservation Price Snapshot`으로 보존한다.
- Daily Rate나 Cancellation Policy가 나중에 바뀌어도 Reservation의 합의 가격과 취소 조건은 바뀌지 않는다.
- `Pending`은 재고 확보 결과가 아직 결정되지 않았음을 뜻하며 결제 대기 상태와 다르다.
- Inventory Hold 성공 전에는 Reservation을 `AwaitingPayment`로 만들 수 없다.

## 모듈과 시간 책임

- Booking 도메인은 Identity, Property, Pricing, Inventory 구현 어셈블리를 참조하지 않는다.
- 애플리케이션 계층은 각 모듈의 계약에서 읽은 값을 Booking 소유 스냅샷 입력으로 변환한다.
- Price Quote 만료와 Hotel, Room Type, Rate Plan의 현재 판매 가능성은 향후 Reservation 생성 유스케이스가 재검증한다.
- 도메인은 시스템 시계를 직접 읽지 않으며 모든 생성 및 전이 시각을 명시적으로 받는다.
- Inventory가 실제로 원자적 Hold를 수행한 뒤에만 재고 확보 성공 전이를 호출한다.

## 장애 대응/롤백

- 불완전하거나 일관되지 않은 가격 스냅샷은 Reservation 생성 전에 거부한다.
- 잘못된 상태 전이는 상태와 기존 시각을 변경하지 않는다.
- 이번 작업은 스키마나 저장 데이터를 변경하지 않으므로 코드와 문서 변경을 되돌리는 것으로 롤백할 수 있다.

## 검증 체크리스트

- [x] Reservation이 완전한 가격 스냅샷과 인증된 Guest 참조를 보존한다.
- [x] Reservation이 `Pending`으로 시작한다.
- [x] 재고 확보 성공과 실패 상태 전이가 유효한 출발 상태에서만 동작한다.
- [x] 가격·취소 조건의 유효 입력, 경계값, 거부 경로 단위 테스트가 통과한다.
- [x] Booking 도메인이 다른 모듈 구현 타입에 의존하지 않는다.
- [x] 전체 솔루션 빌드와 포맷 검증이 통과한다.

검증 명령:

```bash
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj --filter "FullyQualifiedName~HotelBooking.UnitTests.Booking."
dotnet test backend/tests/HotelBooking.UnitTests/HotelBooking.UnitTests.csproj
dotnet test backend/tests/HotelBooking.ArchitectureTests/HotelBooking.ArchitectureTests.csproj
dotnet build backend/HotelBooking.slnx --no-restore
dotnet format backend/HotelBooking.slnx --no-restore --verify-no-changes
git diff --check
```

## 완료 기준(DoD)

- [x] `CONTEXT.md`의 Guest, Reservation, Requested Occupancy, Reservation Price Snapshot 언어를 따른다.
- [x] 구현이 이 문서의 도메인 규칙과 Aggregate 경계를 따른다.
- [x] 새 공개 API에 의도를 설명하는 XML 문서가 있다.
- [x] 필요한 단위 테스트와 아키텍처 테스트가 통과한다.
- [x] 영속성, 애플리케이션 조정, 메시징과 Payment 상태 전이가 범위 밖임을 결과에 명시한다.
