# Metro System Overview

This document provides a comprehensive overview of the metro fare calculation system, including network topology, fare structure, and operational information.

## System Map

The metro system consists of 4 colored lines serving 8 stations across 3 concentric fare zones.

## Zone Structure

The metro system uses a concentric zone structure for fare calculation:

```
┌─────────────────────────────────────────┐
│              Zone C (Outer)            │
│  ┌───────────────────────────────────┐  │
│  │           Zone B (Middle)        │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │      Zone A (Central)      │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

## Fare Structure

### Base Fares by Zone Distance
| Zone Combination | Base Fare |
|------------------|-----------|
| Within same zone | $2.50     |
| 1 zone difference | $2.50     |
| 2 zone difference | $3.75     |
| 3 zone difference | $5.00     |

### Metro Line Multipliers
| Line | Multiplier | Type |
|------|------------|------|
| Red Line (RL) | 1.2x | Express |
| Blue Line (BL) | 1.0x | Local |
| Green Line (GL) | 1.0x | Local |
| Yellow Line (YL) | 0.8x | Shuttle |

### Passenger Discounts
| Passenger Type | Discount |
|----------------|----------|
| Adult | 0% |
| Child | 50% |
| Senior | 30% |
| Student | 20% |
| Disabled | 50% |

## How to Generate Visualizations

The visualizations can be automatically generated using:

```bash
cd src/FareCalculator
dotnet run --visualize
```

This creates:
- `metro-system-map.md` - Mermaid diagram
- `metro-system-ascii.txt` - ASCII art map
- `fare-structure.txt` - Fare calculation guide

### Stations by Zone

**Zone A (Central)**
- Downtown Central (RL, BL) [TRANSFER]
- Uptown North (RL)
- Harbor View (BL)

**Zone B (Middle)**
- Eastside Plaza (BL, GL) [TRANSFER]
- Westwood Terminal (GL)
- University Campus (RL, GL) [TRANSFER]

**Zone C (Outer)**
- Southgate Junction (BL, GL) [TRANSFER]
- Airport Express (YL)

### Time-Based Adjustments
- **Peak Hours**: +25% surcharge (7-9 AM, 5-7 PM weekdays)
- **Off-Peak**: -10% discount (10 PM - 6 AM)

### Transfer Penalties
Different penalties apply based on line combinations:
- Blue ↔ Red: $0.25
- Blue ↔ Green: $0.50  
- Green ↔ Yellow: $0.75

## Generated Files

Running `dotnet run --visualize` creates:
- **metro-system-map.md** - Interactive Mermaid diagram
- **metro-system-ascii.txt** - Text-based network map  
- **fare-structure.txt** - Complete fare calculation reference

For detailed fare calculations and interactive examples, see the generated files in the docs directory. 