# Rails vs Volt - Feature Gap Analysis

Comprehensive comparison of Ruby on Rails 8.x features against Volt Framework 0.2.0.
Features are organized by Rails component with Volt's current status noted.

**Legend:** Implemented | Partial | Missing

---

## 1. Active Record (ORM) → Volt.Data / Volt.Core

### Implemented
- Base model class with auto-incrementing ID, timestamps (CreatedAt/UpdatedAt)
- Soft deletes (DeletedAt) with global query filter
- snake_case column and table naming conventions
- Pluralized table names
- Foreign key conventions (model_name_id)
- Migrations (via EF Core)
- DbContext auto-registration of Model<T> subclasses
- Pagination (Page extension method)
- Database seeding (IVoltSeeder)
- Multi-database support (SQLite, PostgreSQL, SQL Server)

### Partial
- **Validations** - FluentValidation wrapper exists (ValidationBuilder) but no declarative model-level validations like Rails' `validates :name, presence: true`
- **Query interface** - Basic EF Core LINQ; no Rails-like scopes, named scopes, or chainable query DSL

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Callbacks/Lifecycle hooks** | `before_save`, `after_create`, `before_destroy`, etc. | Model lifecycle callbacks that fire on create, update, save, destroy, validate. Volt has `[BeforeAction]`/`[AfterAction]` on controllers but nothing on models. |
| **Associations DSL** | `has_many`, `belongs_to`, `has_one`, `has_many :through`, `has_and_belongs_to_many` | Declarative relationship definitions with automatic eager loading, dependent destroy, counter caches, inverse detection. Volt relies on raw EF Core navigation properties. |
| **Scopes** | `scope :published, -> { where(published: true) }` | Reusable, chainable query fragments defined on the model. |
| **Enum attributes** | `enum :status, [:draft, :published, :archived]` | Declarative enum mapping with query methods and scopes auto-generated. |
| **Dirty tracking** | `changed?`, `name_changed?`, `changes` | Track which attributes changed before save, access previous values. |
| **Optimistic locking** | `lock_version` column | Prevent stale updates with automatic version checking. |
| **Pessimistic locking** | `lock!`, `with_lock` | Database-level row locking for concurrent access. |
| **Single Table Inheritance** | `type` column | Multiple model classes stored in one table with automatic type discrimination. |
| **Polymorphic associations** | `belongs_to :commentable, polymorphic: true` | One model belongs to multiple other models via type+id columns. |
| **Counter caches** | `counter_cache: true` | Auto-maintained count columns to avoid COUNT queries. |
| **Transactions** | `ActiveRecord::Base.transaction` | Declarative transaction blocks. EF Core has SaveChanges but no Rails-style `transaction do...end`. |
| **Nested attributes** | `accepts_nested_attributes_for` | Create/update associated records through the parent model's form. |
| **Serialization** | `serialize :preferences, JSON` | Store structured data (JSON, YAML) in a single column with auto-serialization. |
| **Encryption** | `encrypts :email` | Transparent column-level encryption at rest (Active Record Encryption). |
| **Strict loading** | `strict_loading!` | Raise errors on lazy loading to prevent N+1 queries in development. |
| **Query methods** | `where.not`, `or`, `find_each`, `find_in_batches`, `pluck`, `pick` | Rich query interface beyond basic LINQ. Batch processing for large datasets. |
| **Connection switching** | `connects_to` | Automatic read/write splitting across replica databases. |
| **Migration helpers** | `add_index`, `add_reference`, `change_column_default`, `reversible` | Rich migration DSL with reversibility, `change` method that auto-generates down migration. |

---

## 2. Action Controller → Volt.Web

### Implemented
- ResourceController<T> with 7 RESTful actions (Index, Show, New, Create, Edit, Update, Destroy)
- ApiResourceController<T> for JSON APIs
- Strong parameters (PermittedParams / PermittedParamsFilter)
- Flash messages
- CSRF protection (ValidateAntiForgeryToken)
- Method override support (PUT/DELETE via hidden form field)
- HTMX integration (request detection, response headers)

### Partial
- **Filters/Callbacks** - `[BeforeAction]` and `[AfterAction]` attributes exist but limited compared to Rails' `before_action`, `after_action`, `around_action` with `:only`, `:except`, `skip_before_action`

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Around actions** | `around_action` | Wrap controller action execution (e.g., for timing, transactions). |
| **Action callbacks with options** | `before_action :auth, only: [:create, :update]` | Filter which actions a callback applies to with `:only` and `:except`. |
| **Skip callbacks** | `skip_before_action :verify_auth` | Override inherited callbacks in derived controllers. |
| **Rescue from** | `rescue_from ActiveRecord::RecordNotFound` | Declarative exception handling per controller or globally. |
| **Respond to / format** | `respond_to do \|format\| format.html; format.json end` | Content negotiation - serve different formats from one action. |
| **Streaming** | `ActionController::Live`, `send_data`, `send_file` | Stream responses for large files or server-sent events. |
| **ETag / conditional GET** | `stale?`, `fresh_when` | HTTP caching with automatic 304 Not Modified responses. |
| **Cookie management** | `cookies`, `cookies.encrypted`, `cookies.signed` | Typed cookie access with encryption and signing. |
| **Session management** | `session[:user_id]` | Configurable session stores (cookie, cache, database). |
| **Request variants** | `request.variant = :mobile` | Serve different templates based on device type. |
| **Default URL options** | `default_url_options` | Set default host/protocol for URL generation. |
| **Parameter wrapping** | `wrap_parameters` | Auto-wrap JSON params in a root key matching the controller name. |

---

## 3. Action View → Razor Views

### Implemented
- Razor views (.cshtml) for HTML rendering
- Layouts
- Partial views (_Form.cshtml)
- Tag helpers (VoltFormFor, VoltLinkTo)
- Form generation with method override
- Scaffolded views (Index, Show, New, Edit, _Form)

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **View helpers** | `number_to_currency`, `time_ago_in_words`, `pluralize`, `truncate` | Rich library of formatting helpers for dates, numbers, text, URLs. |
| **Form builder** | `form_with`, `fields_for`, `collection_select`, `date_select` | Comprehensive form builder with nested model forms, collection selects, date/time pickers. |
| **Asset pipeline** | Propshaft / Import Maps | CSS/JS bundling, fingerprinting, minification, import maps for zero-build JavaScript. |
| **Content tag helpers** | `content_tag`, `tag.div`, `tag.p` | Programmatic HTML generation in helpers. |
| **View components** | ViewComponent gem (widely used) | Reusable, testable UI components with their own templates and logic. |
| **Turbo Frames** | `turbo_frame_tag` | Decompose pages into independent frames that can be lazily loaded or updated. |
| **Turbo Streams** | `turbo_stream.append`, `.replace`, `.remove` | Server-driven HTML updates over WebSocket or HTTP responses. |
| **Stimulus controllers** | `data-controller`, `data-action` | Lightweight JavaScript framework for progressive enhancement. |
| **Content for / yield** | `content_for :sidebar`, `yield :sidebar` | Named content blocks that child views can inject into layouts. |
| **Collection rendering** | `render @articles` | Automatic partial rendering for collections with spacer templates. |
| **Jbuilder / JSON views** | `index.json.jbuilder` | Template-based JSON response construction. |
| **Preview templates** | Mailer previews, ViewComponent previews | Preview rendered output in development without full request cycle. |

---

## 4. Routing

### Implemented
- RESTful resource routes (MapVoltResources<T>)
- Attribute routing ([Route], [HttpGet], etc.)
- Conventional routing ({controller}/{action}/{id})
- `volt routes` CLI command

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Nested resources** | `resources :articles do; resources :comments; end` | URL nesting like `/articles/1/comments/2`. |
| **Namespace / scope** | `namespace :admin`, `scope :api` | Group routes under URL prefix and/or module. |
| **Route constraints** | `constraints(subdomain: 'api')` | Restrict routes by subdomain, format, custom logic. |
| **Member / collection routes** | `member { get :preview }`, `collection { get :search }` | Add custom actions to resource routes. |
| **Shallow nesting** | `resources :articles, shallow: true` | Nest only collection routes, flatten member routes. |
| **Concerns** | `concern :commentable` | Reusable route definitions shared across resources. |
| **Root route** | `root 'home#index'` | Declarative root URL mapping. |
| **Route helpers** | `articles_path`, `new_article_url` | Auto-generated named route helper methods. |
| **Direct / resolve** | `direct(:homepage) { "https://..." }` | Custom URL helpers and polymorphic route resolution. |

---

## 5. Active Storage → Volt.Storage

### Implemented
- IStorageService abstraction
- DiskStorageService (local filesystem)
- S3StorageService (Amazon S3)
- VoltAttachment model (filename, content_type, byte_size, key, checksum)
- [HasOneAttached] / [HasManyAttached] attributes
- File upload handling in scaffold controllers
- Storage middleware for serving files

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Image variants/transforms** | `image.variant(resize_to_limit: [100, 100])` | On-the-fly image resizing, cropping, format conversion via ImageMagick/libvips. |
| **Direct uploads** | `direct_upload_url` | Client-side direct-to-cloud uploads bypassing the server. |
| **Mirrors** | `mirror: true` | Write to multiple storage services simultaneously for migration. |
| **Previews** | `blob.preview(resize_to_limit: [300, 300])` | Generate preview images for PDFs and videos. |
| **Content analysis** | Automatic metadata extraction | Auto-detect dimensions, duration, content type on upload. |
| **Proxy mode** | `rails_storage_proxy_path` | Serve private files through the app with auth checks. |
| **Azure / GCS backends** | Azure Storage, Google Cloud Storage | Additional cloud storage providers. |

---

## 6. Action Mailer → Volt.Mailer

### Implemented
- VoltMailer base class with fluent API (To, From, Subject, Send)
- Razor view templates for emails (Views/Mailers/)
- SMTP configuration (host, port, SSL, credentials)
- Development preview UI (/volt/mailers)
- MailHog integration for local development

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Multipart emails** | `mail` auto-detects .html.erb and .text.erb | Automatic HTML + plain text multipart emails from separate templates. |
| **Attachments** | `attachments['file.pdf'] = File.read(path)` | Attach files to outgoing emails. |
| **Inline attachments** | `attachments.inline['logo.png']` | Embed images directly in HTML emails. |
| **Delivery methods** | `:smtp`, `:sendmail`, `:test`, `:file` | Multiple delivery backends including test interceptor. |
| **Interceptors / observers** | `register_interceptor`, `register_observer` | Hook into email delivery for logging, modification, or analytics. |
| **Parameterized mailers** | `with(user: @user).welcome` | Share common setup across multiple mailer methods. |
| **I18n subject** | `default_i18n_subject` | Automatic email subject from locale files. |
| **Delivery later** | `deliver_later` | Queue emails for background delivery via Active Job. |

---

## 7. Action Mailbox (Inbound Email)

### Missing (entirely)

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Inbound email routing** | `routing /receipts/i => :receipts` | Route incoming emails to mailbox classes based on patterns. |
| **Email processing** | `InboxController < ApplicationMailbox` | Parse and process inbound emails (forward, reply, bounce). |
| **Provider integrations** | Mailgun, SendGrid, Postmark, Amazon SES | Webhook-based inbound email from major providers. |

---

## 8. Active Job → Volt.Jobs

### Implemented
- VoltJob<TPayload> base class with typed payloads
- VoltJobWithoutPayload for parameterless jobs
- Job queue (IJobQueue with Enqueue and Schedule)
- Scheduler (VoltSchedulerConfig)
- Auto-discovery and DI registration
- Built on Coravel

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Retry mechanism** | `retry_on`, `discard_on` | Automatic retry with backoff on specific exceptions. |
| **Priority queues** | `queue_as :high_priority` | Assign jobs to named queues with different priorities. |
| **Job callbacks** | `before_enqueue`, `after_perform`, `around_perform` | Lifecycle hooks on job execution. |
| **Multiple backends** | Sidekiq, Resque, Delayed Job, Solid Queue | Swappable queue backends. Coravel is the only option currently. |
| **Bulk enqueue** | `perform_all_later` | Enqueue multiple jobs in a single operation. |
| **Job serialization** | GlobalID | Serialize ActiveRecord objects as job arguments. |
| **Recurring jobs** | `Solid Queue` recurring tasks | Cron-like recurring job definitions. |

---

## 9. Action Cable → Volt.RealTime

### Implemented
- VoltChannel base class (extends SignalR Hub)
- Channel broadcasting (IChannelBroadcaster)
- Group-based broadcasting (BroadcastTo)
- Auto-discovery and mapping (/volt/channels/{name})
- Configuration (base path, keep-alive, timeouts)

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Channel authorization** | `reject_unauthorized_connection` | Authenticate WebSocket connections on connect. |
| **Stream subscriptions** | `stream_from "chat_#{room}"`, `stream_for @room` | Subscribe to named streams with automatic cleanup. |
| **Channel callbacks** | `before_subscribe`, `after_subscribe` | Lifecycle hooks on subscription events. |
| **Client-side framework** | `@rails/actioncable` JavaScript package | Full client-side JS library with auto-reconnection, subscription management. |
| **Testing helpers** | `assert_broadcast_on` | Test helpers for verifying broadcasts. |

---

## 10. Active Support (Core Extensions)

### Missing (entirely)

Rails' Active Support provides a massive utility library. Key missing features:

| Feature | Description |
|---------|-------------|
| **Time extensions** | `2.days.ago`, `3.hours.from_now`, `Time.zone`, `beginning_of_day` |
| **String extensions** | `"hello_world".camelize`, `"Article".pluralize`, `"article".classify`, `"Hello World".parameterize` |
| **Number formatting** | `number_to_currency`, `number_to_human_size`, `number_to_percentage` |
| **Hash extensions** | `deep_merge`, `except`, `slice`, `transform_keys`, `symbolize_keys` |
| **Inflector** | Pluralization, singularization, titleization, humanization rules |
| **Memoization** | `delegate`, `memoize` patterns |
| **Concern module** | Reusable module system for models and controllers |
| **Notifications / Instrumentation** | `ActiveSupport::Notifications.instrument` for event pub/sub |
| **Caching abstraction** | `Rails.cache.fetch`, memory/Redis/memcached stores |
| **MessageEncryptor / MessageVerifier** | Signed and encrypted message passing |
| **CurrentAttributes** | Thread-safe request-scoped global state (`Current.user`) |
| **Configurable** | `config_accessor` for module-level configuration |

---

## 11. Testing → Volt.Testing

### Implemented
- VoltTestBase<TProgram, TContext> integration test base
- In-memory SQLite per test
- Factory pattern (Define, Build, Create, BuildList, CreateList)
- HttpClient for request testing
- Direct database access in tests

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **System tests** | `ApplicationSystemTestCase` (Capybara) | Browser-based end-to-end tests with screenshots on failure. |
| **Fixtures** | `fixtures :articles` | YAML-based test data loaded into database before tests. |
| **Controller test helpers** | `get :index`, `assert_response :success`, `assert_redirected_to` | Simplified controller action testing without HTTP. |
| **Mailer test helpers** | `assert_emails 1`, `assert_enqueued_emails` | Assert email sending count and content. |
| **Job test helpers** | `assert_enqueued_with(job: MyJob)` | Assert jobs were enqueued with specific arguments. |
| **Channel test helpers** | `assert_broadcast_on` | Test Action Cable broadcasts. |
| **Parallel testing** | `parallelize(workers: 4)` | Run tests in parallel processes for speed. |
| **Test coverage** | Built-in coverage reporting | Coverage tracking integration. |

---

## 12. Security

### Implemented
- CSRF protection (antiforgery tokens)
- Strong parameters (mass-assignment protection)
- ASP.NET Core Identity (authentication via Volt.Auth)
- Password policy configuration
- Account lockout
- Session timeout

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Content Security Policy** | `content_security_policy` DSL | Configure CSP headers declaratively per-controller. |
| **Permissions Policy** | `permissions_policy` | Control browser feature access (camera, microphone, etc.). |
| **XSS protection** | Auto-escaped output, `sanitize` helper | While Razor auto-escapes, no sanitize helper for rich HTML. |
| **SQL injection protection** | Parameterized queries by default | EF Core handles this, but no additional query safety layer. |
| **Credential encryption** | `rails credentials:edit` | Encrypted credentials file for secrets management. |
| **Has secure password** | `has_secure_password` | Built-in bcrypt password hashing on any model. (Identity handles this for User only.) |
| **Rate limiting** | `rate_limit to: 10, within: 1.minute` | Built-in request rate limiting per controller action. |
| **IP blocking** | `ActionDispatch::RemoteIp` | Request IP validation and spoofing prevention. |

---

## 13. Caching

### Missing (entirely)

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Fragment caching** | `cache @article do` | Cache rendered view fragments with automatic expiration. |
| **Russian doll caching** | Nested `cache` calls with `touch: true` | Nested fragment caches that auto-expire when inner content changes. |
| **Low-level caching** | `Rails.cache.fetch("key")` | Key-value cache store abstraction (memory, Redis, Memcached). |
| **HTTP caching** | `expires_in`, `stale?`, `fresh_when` | ETag, Last-Modified, Cache-Control header management. |
| **Counter caching** | `counter_cache: true` on associations | Auto-maintained count columns to avoid COUNT queries. |
| **Query caching** | Automatic per-request SQL cache | Cache duplicate SQL queries within a single request. |
| **Cache store backends** | Memory, Redis, Memcached, File, Null | Pluggable cache storage backends. |
| **Solid Cache** | Database-backed cache store | Use the database itself as cache store for simpler deployments. |

---

## 14. Internationalization (I18n)

### Missing (entirely)

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Locale files** | `config/locales/en.yml` | YAML/JSON locale files for translations. |
| **Translation helper** | `t('articles.title')`, `I18n.t` | Lookup translations by key with interpolation. |
| **Locale switching** | `I18n.locale = :fr` | Per-request locale based on URL, header, or cookie. |
| **Pluralization rules** | `count:` parameter in translations | Language-specific pluralization. |
| **Date/time localization** | `l(date, format: :long)` | Locale-aware date and time formatting. |
| **Model translations** | `human_attribute_name` | Translated model/attribute names for forms and errors. |
| **Fallback chain** | `I18n.fallbacks` | Fall back to default locale when translation missing. |

---

## 15. Action Text (Rich Text)

### Missing (entirely)

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Rich text editor** | Trix editor integration | WYSIWYG editor with formatting, links, lists, attachments. |
| **Rich text model** | `has_rich_text :body` | Store rich text content with embedded attachments. |
| **Embedded attachments** | Inline images/files in rich text | Drag-and-drop files into rich text that auto-upload via Active Storage. |
| **Content rendering** | Auto-sanitized HTML output | Render rich text safely in views. |

---

## 16. Hotwire / Turbo

### Partial
- HTMX integration exists (HtmxMiddleware, response helpers)
- This is Volt's equivalent of Turbo but using HTMX instead

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Turbo Drive** | Full-page navigation interception | HTMX `hx-boost` equivalent exists but not auto-applied to all links. |
| **Turbo Frames** | `<turbo-frame>` | Scoped page regions that load/update independently. No HTMX frame equivalent built-in. |
| **Turbo Streams** | `<turbo-stream action="append">` | Server-push HTML mutations (append, prepend, replace, remove, update). |
| **Broadcast integration** | `broadcasts_to` on models | Automatic Turbo Stream broadcasts when models change. |
| **Stimulus** | JavaScript controllers | Lightweight JS framework for progressive enhancement. No built-in JS framework. |
| **Native bridge** | Turbo Native (iOS/Android) | Mobile app WebView integration. |

---

## 17. Credentials / Secrets Management

### Missing (entirely)

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Encrypted credentials** | `rails credentials:edit` | Edit encrypted YAML file; decrypted at runtime with master key. |
| **Per-environment credentials** | `credentials/production.yml.enc` | Separate encrypted files per environment. |
| **Master key** | `config/master.key` | Single key file (gitignored) to decrypt credentials. |
| **Secret key base** | `secret_key_base` | Application-wide signing/encryption key. |

---

## 18. Middleware

### Implemented
- PendingMigrationsMiddleware (like Rails' migration pending page)
- HtmxMiddleware
- Static files middleware
- Storage file serving middleware
- Standard ASP.NET Core middleware (auth, CORS, etc.)

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Middleware stack DSL** | `config.middleware.use`, `insert_before`, `delete` | Declarative middleware ordering and manipulation. |
| **Request logging** | `Rails::Rack::Logger` | Structured request/response logging with timing. |
| **Request ID** | `ActionDispatch::RequestId` | Assign unique ID to each request for tracing. |
| **SSL enforcement** | `force_ssl` | Auto-redirect HTTP to HTTPS with HSTS. |
| **Exception notification** | Error reporting integrations | Built-in error reporting with pluggable subscribers. |

---

## 19. API Mode

### Implemented
- ApiResourceController<T> with JSON responses
- `volt new --api` template
- Pagination in API responses

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **API versioning** | Namespace/module versioning | URL or header-based API versioning (`/api/v1/articles`). |
| **JSON serialization** | ActiveModelSerializers, Jbuilder | Configurable JSON output with field selection, nested includes, sparse fieldsets. |
| **CORS configuration** | `Rack::Cors` | Cross-origin resource sharing setup. ASP.NET Core has this but no Volt wrapper. |
| **API authentication** | Token-based auth, JWT | API token or JWT authentication. Identity is session-based only. |
| **Rate limiting** | `Rack::Attack` | Per-endpoint rate limiting for API protection. |
| **Pagination links** | `Link` header, cursor pagination | Standard pagination response format (next/prev links, cursors). |
| **OpenAPI / Swagger** | `rswag` gem | Auto-generated API documentation from routes and models. |

---

## 20. Database Features

### Implemented
- Migrations (EF Core)
- Multiple providers (SQLite, PostgreSQL, SQL Server)
- `volt db migrate`, `rollback`, `status`, `reset`, `seed`, `console`
- Auto-timestamps
- Soft deletes

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Schema file** | `db/schema.rb` | Authoritative schema dump representing current database structure. |
| **Structure SQL** | `db/structure.sql` | Raw SQL schema dump for database-specific features. |
| **Reversible migrations** | `change` method auto-generates rollback | Automatic rollback generation from migration definitions. |
| **Multi-database** | `connects_to database: { writing: :primary, reading: :replica }` | Read/write splitting, automatic connection switching. |
| **Database encryption** | `encrypts :ssn, deterministic: true` | Transparent application-level encryption for sensitive columns. |
| **Composite primary keys** | `self.primary_key = [:shop_id, :id]` | Multi-column primary keys with full query/association support. |
| **Annotate models** | Schema comments in model files | Auto-generated schema documentation in model source files. |

---

## 21. CLI Features

### Implemented
- `volt new` (with --database, --api options)
- `volt generate` (model, scaffold, controller, migration, job, mailer, channel, ai-context)
- `volt db` (migrate, rollback, seed, reset, status, console, provider, use)
- `volt server` (with port, https, open, verbose)
- `volt routes`
- `volt console`
- `volt destroy`

### Missing

| Feature | Rails Equivalent | Description |
|---------|-----------------|-------------|
| **Generator for tests** | `rails generate test_unit:model` | Generate test files independently. |
| **Generator for views** | `rails generate erb:scaffold` | Generate view files independently. |
| **Generator for helper** | `rails generate helper` | Generate helper classes. |
| **Generator for task** | `rails generate task` | Generate Rake task equivalents. |
| **Runner** | `rails runner "puts User.count"` | Execute one-off scripts in app context. |
| **Notes** | `rails notes` | Extract TODO/FIXME/OPTIMIZE annotations from source. |
| **Stats** | `rails stats` | LOC and test ratio statistics. |
| **Middleware list** | `rails middleware` | Show ordered middleware stack. |
| **DB encryption setup** | `rails db:encryption:init` | Generate encryption keys for Active Record Encryption. |

---

## Priority Recommendations

### High Priority (Core Rails Parity)
1. **Model validations DSL** - Declarative validations are fundamental to Rails DX
2. **Model callbacks** - `before_save`, `after_create`, etc. on models
3. **Associations DSL** - `has_many`, `belongs_to` with dependent destroy, eager loading hints
4. **Scopes** - Reusable query fragments on models
5. **Nested resources** - `/articles/1/comments` routing
6. **Fragment caching** - Essential for production performance
7. **Credential management** - Encrypted secrets storage
8. **Content negotiation** - Respond to HTML vs JSON from same action

### Medium Priority (Developer Experience)
9. **Model lifecycle hooks** - Dirty tracking, conditional callbacks
10. **Enum attributes** - Declarative enums with auto-generated methods
11. **Test helpers** - Controller, mailer, job test assertions
12. **View helpers** - Date formatting, number formatting, text utilities
13. **Nested attributes** - Multi-model forms
14. **Mailer attachments** - File attachments in emails
15. **Job retry/priority** - Retry on failure, queue prioritization
16. **Rate limiting** - Built-in request throttling
17. **I18n basics** - Translation files and lookup helper

### Lower Priority (Advanced Features)
18. **Action Text** - Rich text editing
19. **Turbo Streams** - Server-push HTML updates
20. **Inbound email** - Action Mailbox equivalent
21. **Image variants** - On-the-fly image processing
22. **Direct uploads** - Client-to-cloud file uploads
23. **Multi-database** - Read/write splitting
24. **API versioning** - URL-based API versions
25. **Composite primary keys** - Multi-column PKs
