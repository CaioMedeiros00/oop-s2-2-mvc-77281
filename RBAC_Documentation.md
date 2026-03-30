# Role-Based Access Control (RBAC) Documentation

## Overview
The Food Tracker application implements three distinct roles with different permission levels:
- **Admin** - Full system access
- **Inspector** - Can perform inspections and manage follow-ups
- **Viewer** - Read-only access to all data

## Role Permissions Matrix

| Feature | Admin | Inspector | Viewer |
|---------|-------|-----------|--------|
| **Premises** | | | |
| View List | ✅ | ✅ | ✅ |
| View Details | ✅ | ✅ | ✅ |
| Search | ✅ | ✅ | ✅ |
| Create | ✅ | ❌ | ❌ |
| Edit | ✅ | ❌ | ❌ |
| Delete | ✅ | ❌ | ❌ |
| **Inspections** | | | |
| View List | ✅ | ✅ | ✅ |
| View Details | ✅ | ✅ | ✅ |
| Create | ✅ | ✅ | ❌ |
| Edit | ✅ | ✅ | ❌ |
| Delete | ✅ | ❌ | ❌ |
| **Follow-Ups** | | | |
| View List | ✅ | ✅ | ✅ |
| Create | ✅ | ✅ | ❌ |
| Edit | ✅ | ✅ | ❌ |
| Delete | ✅ | ❌ | ❌ |
| **Dashboard** | | | |
| View Statistics | ✅ | ✅ | ✅ |
| Filter Data | ✅ | ✅ | ✅ |

## Role Descriptions

### Admin Role
**Purpose:** System administrators with full control over all data and operations.

**Capabilities:**
- Full CRUD operations on all entities (Premises, Inspections, Follow-Ups)
- Can delete any record in the system
- Manages system-wide data and corrects errors
- Access to all features and functionalities

**Typical Use Case:** IT administrators, system managers, or supervisors who need complete control.

### Inspector Role
**Purpose:** Food safety inspectors who conduct inspections and manage follow-up actions.

**Capabilities:**
- View all premises, inspections, and follow-ups
- Create new inspections
- Edit existing inspections
- Create and edit follow-up actions
- Cannot delete records (for audit trail integrity)
- Cannot manage premises (Admin-only function)

**Typical Use Case:** Field inspectors who conduct food safety inspections and track compliance.

### Viewer Role
**Purpose:** Read-only users who need to view data without making changes.

**Capabilities:**
- View all premises and their details
- View all inspections and their outcomes
- View all follow-up actions and their status
- Access dashboard statistics and filters
- Search for premises
- Cannot create, edit, or delete any data

**Typical Use Case:** Management, auditors, or stakeholders who need visibility into food safety data without operational responsibilities.

## Security Implementation

### Controller-Level Authorization
All controllers use the `[Authorize]` attribute to require authentication. Specific actions use role-based authorization:

```csharp
[Authorize(Roles = "Admin")]              // Admin only
[Authorize(Roles = "Admin,Inspector")]     // Admin or Inspector
[Authorize]                                // All authenticated users
```

### View-Level Permission Checks
Views use `User.IsInRole()` to conditionally display UI elements:

```razor
@if (User.IsInRole("Admin"))
{
    <a asp-action="Delete" class="btn btn-danger">Delete</a>
}
```

### Permission Hierarchy
```
Admin (Full Access)
  ├── All Inspector permissions
  ├── Delete all records
  └── Manage premises
  
Inspector (Operational Access)
  ├── All Viewer permissions
  ├── Create/Edit inspections
  └── Create/Edit follow-ups
  
Viewer (Read-Only Access)
  ├── View all data
  ├── Search capabilities
  └── Access dashboard
```

## Default Test Accounts

The system is seeded with the following test accounts (see `DbSeeder.cs`):

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@foodsafety.local | Admin123! |
| Inspector | inspector@foodsafety.local | Inspector123! |
| Viewer | viewer@foodsafety.local | Viewer123! |

## Audit Trail

All actions are logged with user information:
- User login/logout events
- Record creation, updates, and deletions
- Access denied attempts
- Failed login attempts

The logging system tracks who performed what action and when, ensuring accountability across all roles.

## Best Practices

1. **Principle of Least Privilege:** Users are assigned the minimum role necessary for their job function
2. **Separation of Duties:** Inspectors can't delete records; only Admins can
3. **Audit Trail:** All actions are logged for compliance and security
4. **UI Consistency:** Buttons/links are hidden from users who don't have permission
5. **Server-Side Validation:** Authorization is enforced at the controller level, not just in the UI
