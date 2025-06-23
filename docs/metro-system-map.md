```mermaid
flowchart TD
    %% Metro System Map - Concentric Zone Layout

    %% Concentric Zone Layout
    subgraph ZoneC ["🟠 Zone C - Outer Ring"]
        subgraph ZoneB ["🟢 Zone B - Middle Ring"]
            subgraph ZoneA ["🔵 Zone A - Central"]
                S1["Downtown<br/>Central"]
                S2["Uptown<br/>North"]
                S8["Harbor<br/>View"]
            end
            S3["Eastside<br/>Plaza"]
            S4["Westwood<br/>Terminal"]
            S7["University<br/>Campus"]
        end
        S5["Southgate<br/>Junction"]
        S6["Airport<br/>Express"]
    end

    %% Metro Line Routes - Colored Lines
    %% Red Line - Express Service
    S1 ==>|RL| S2
    S2 ==>|RL| S7
    %% Blue Line - Local Service
    S1 ==>|BL| S3
    S3 ==>|BL| S5
    S5 ==>|BL| S8
    %% Green Line - Local Service
    S3 ==>|GL| S4
    S4 ==>|GL| S5
    S5 ==>|GL| S7

    %% Zone Styling
    style ZoneA fill:#e3f2fd,stroke:#1976d2,stroke-width:3px
    style ZoneB fill:#e8f5e8,stroke:#388e3c,stroke-width:3px
    style ZoneC fill:#fff3e0,stroke:#f57c00,stroke-width:3px

    %% Station Styling
    style S1 fill:#1e40af,stroke:#1e3a8a,stroke-width:3px,color:#ffffff
    style S2 fill:#3b82f6,stroke:#1e3a8a,stroke-width:3px,color:#ffffff
    style S8 fill:#3b82f6,stroke:#1e3a8a,stroke-width:3px,color:#ffffff
    style S3 fill:#15803d,stroke:#14532d,stroke-width:3px,color:#ffffff
    style S4 fill:#22c55e,stroke:#14532d,stroke-width:3px,color:#ffffff
    style S7 fill:#15803d,stroke:#14532d,stroke-width:3px,color:#ffffff
    style S5 fill:#ea580c,stroke:#9a3412,stroke-width:3px,color:#ffffff
    style S6 fill:#f97316,stroke:#9a3412,stroke-width:3px,color:#ffffff

    %% Metro Line Colors
    linkStyle 0 stroke:#dc2626,stroke-width:8px
    linkStyle 1 stroke:#dc2626,stroke-width:8px
    linkStyle 2 stroke:#2563eb,stroke-width:8px
    linkStyle 3 stroke:#2563eb,stroke-width:8px
    linkStyle 4 stroke:#2563eb,stroke-width:8px
    linkStyle 5 stroke:#16a34a,stroke-width:8px
    linkStyle 6 stroke:#16a34a,stroke-width:8px
    linkStyle 7 stroke:#16a34a,stroke-width:8px
```
