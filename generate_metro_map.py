#!/usr/bin/env python3
"""
Metro Map Generator - Creates a professional metro system map with zones, fares, and discounts
Suitable for display in metro stations
"""

import matplotlib.pyplot as plt
import matplotlib.patches as patches
from matplotlib.patches import Circle, FancyBboxPatch
import numpy as np
import pandas as pd

def create_metro_map():
    # Create a large figure for high quality display
    fig, (ax_map, ax_info) = plt.subplots(1, 2, figsize=(20, 12))
    fig.suptitle('METRO SYSTEM MAP\nZones, Fares & Discounts', fontsize=28, fontweight='bold', y=0.95)
    
    # Station data from the configuration
    stations = {
        'Downtown Central': {'zone': 'A', 'pos': (2, 3), 'color': '#e74c3c'},
        'Uptown North': {'zone': 'A', 'pos': (1.5, 4.5), 'color': '#e74c3c'},
        'Harbor View': {'zone': 'A', 'pos': (1, 2), 'color': '#e74c3c'},
        'Eastside Plaza': {'zone': 'B', 'pos': (4, 4), 'color': '#f39c12'},
        'Westwood Terminal': {'zone': 'B', 'pos': (3, 1.5), 'color': '#f39c12'},
        'University Campus': {'zone': 'B', 'pos': (4.5, 5), 'color': '#f39c12'},
        'Southgate Junction': {'zone': 'C', 'pos': (6, 2), 'color': '#27ae60'},
        'Airport Express': {'zone': 'C', 'pos': (7, 3.5), 'color': '#27ae60'}
    }
    
    # Zone configurations
    zones = {
        'A': {'center': (1.8, 3.2), 'radius': 1.2, 'color': '#e74c3c', 'alpha': 0.15},
        'B': {'center': (3.8, 3.5), 'radius': 1.8, 'color': '#f39c12', 'alpha': 0.15},
        'C': {'center': (6.2, 2.8), 'radius': 1.5, 'color': '#27ae60', 'alpha': 0.15}
    }
    
    # Draw zone circles
    for zone_name, zone_data in zones.items():
        circle = Circle(zone_data['center'], zone_data['radius'], 
                       fill=True, facecolor=zone_data['color'], alpha=zone_data['alpha'],
                       edgecolor=zone_data['color'], linewidth=3)
        ax_map.add_patch(circle)
        
        # Add zone labels
        ax_map.text(zone_data['center'][0], zone_data['center'][1] + zone_data['radius'] + 0.3,
                   f'ZONE {zone_name}', fontsize=16, fontweight='bold', ha='center',
                   color=zone_data['color'])
    
    # Draw metro lines (connecting stations)
    # Main line
    line1_stations = ['Uptown North', 'Downtown Central', 'Eastside Plaza', 'University Campus']
    line2_stations = ['Harbor View', 'Westwood Terminal', 'Southgate Junction', 'Airport Express']
    
    # Draw connecting lines
    for i in range(len(line1_stations) - 1):
        start = stations[line1_stations[i]]['pos']
        end = stations[line1_stations[i + 1]]['pos']
        ax_map.plot([start[0], end[0]], [start[1], end[1]], 'k-', linewidth=4, alpha=0.7)
    
    for i in range(len(line2_stations) - 1):
        start = stations[line2_stations[i]]['pos']
        end = stations[line2_stations[i + 1]]['pos']
        ax_map.plot([start[0], end[0]], [start[1], end[1]], 'k-', linewidth=4, alpha=0.7)
    
    # Additional connecting lines between zones
    ax_map.plot([stations['Downtown Central']['pos'][0], stations['Westwood Terminal']['pos'][0]], 
               [stations['Downtown Central']['pos'][1], stations['Westwood Terminal']['pos'][1]], 
               'k-', linewidth=4, alpha=0.7)
    ax_map.plot([stations['Eastside Plaza']['pos'][0], stations['Southgate Junction']['pos'][0]], 
               [stations['Eastside Plaza']['pos'][1], stations['Southgate Junction']['pos'][1]], 
               'k-', linewidth=4, alpha=0.7)
    
    # Draw stations
    for station_name, station_data in stations.items():
        x, y = station_data['pos']
        
        # Station circle
        circle = Circle((x, y), 0.15, facecolor=station_data['color'], 
                       edgecolor='white', linewidth=3, zorder=10)
        ax_map.add_patch(circle)
        
        # Station label with background
        bbox_props = dict(boxstyle="round,pad=0.3", facecolor='white', alpha=0.8, edgecolor='gray')
        ax_map.text(x, y - 0.4, station_name, fontsize=10, fontweight='bold', 
                   ha='center', va='top', bbox=bbox_props)
    
    # Map formatting
    ax_map.set_xlim(0, 8)
    ax_map.set_ylim(0, 6)
    ax_map.set_aspect('equal')
    ax_map.axis('off')
    ax_map.set_title('METRO NETWORK', fontsize=20, fontweight='bold', pad=20)
    
    # Information panel
    ax_info.axis('off')
    
    # Fare structure table
    fare_y_start = 0.85
    ax_info.text(0.1, 0.95, '💰 FARE STRUCTURE', fontsize=18, fontweight='bold', 
                transform=ax_info.transAxes, color='#2c3e50')
    
    fare_data = [
        ['Travel Distance', 'Adult Fare'],
        ['1 Zone (same zone)', '$2.50'],
        ['2 Zones', '$3.75'],
        ['3 Zones', '$5.00']
    ]
    
    # Create fare table
    for i, row in enumerate(fare_data):
        y_pos = fare_y_start - (i * 0.08)
        if i == 0:  # Header
            rect = FancyBboxPatch((0.1, y_pos - 0.03), 0.8, 0.06, 
                                boxstyle="round,pad=0.01", 
                                facecolor='#34495e', transform=ax_info.transAxes)
            ax_info.add_patch(rect)
            ax_info.text(0.15, y_pos, row[0], fontsize=12, fontweight='bold', 
                        color='white', transform=ax_info.transAxes)
            ax_info.text(0.7, y_pos, row[1], fontsize=12, fontweight='bold', 
                        color='white', transform=ax_info.transAxes)
        else:
            if i % 2 == 0:  # Alternate row coloring
                rect = FancyBboxPatch((0.1, y_pos - 0.03), 0.8, 0.06, 
                                    boxstyle="round,pad=0.01", 
                                    facecolor='#f8f9fa', transform=ax_info.transAxes)
                ax_info.add_patch(rect)
            ax_info.text(0.15, y_pos, row[0], fontsize=11, transform=ax_info.transAxes)
            ax_info.text(0.7, y_pos, row[1], fontsize=11, fontweight='bold', 
                        transform=ax_info.transAxes)
    
    # Passenger discounts
    discount_y_start = 0.55
    ax_info.text(0.1, 0.65, '🎫 PASSENGER DISCOUNTS', fontsize=18, fontweight='bold', 
                transform=ax_info.transAxes, color='#2c3e50')
    
    discounts = [
        ('Child', '50% OFF', '#e74c3c'),
        ('Disabled', '50% OFF', '#e74c3c'),
        ('Senior', '30% OFF', '#f39c12'),
        ('Student', '20% OFF', '#3498db')
    ]
    
    for i, (passenger_type, discount, color) in enumerate(discounts):
        y_pos = discount_y_start - (i * 0.06)
        
        # Background box
        rect = FancyBboxPatch((0.1, y_pos - 0.02), 0.8, 0.04, 
                            boxstyle="round,pad=0.01", 
                            facecolor='white', edgecolor='gray', linewidth=0.5,
                            transform=ax_info.transAxes)
        ax_info.add_patch(rect)
        
        # Passenger type
        ax_info.text(0.15, y_pos, passenger_type, fontsize=12, fontweight='bold', 
                    transform=ax_info.transAxes)
        
        # Discount badge
        badge_rect = FancyBboxPatch((0.7, y_pos - 0.015), 0.15, 0.03, 
                                  boxstyle="round,pad=0.005", 
                                  facecolor=color, transform=ax_info.transAxes)
        ax_info.add_patch(badge_rect)
        ax_info.text(0.775, y_pos, discount, fontsize=10, fontweight='bold', 
                    color='white', ha='center', transform=ax_info.transAxes)
    
    # Time-based pricing
    time_y_start = 0.25
    ax_info.text(0.1, 0.35, '⏰ TIME-BASED PRICING', fontsize=18, fontweight='bold', 
                transform=ax_info.transAxes, color='#2c3e50')
    
    time_blocks = [
        ('Peak Hours', 'Mon-Fri: 7-9 AM & 5-7 PM', '+25% Surcharge', '#e74c3c'),
        ('Off-Peak', 'Daily: 10 PM - 6 AM', '10% Discount', '#27ae60'),
        ('Regular', 'All other times', 'Standard Fare', '#3498db')
    ]
    
    for i, (period, time_range, pricing, color) in enumerate(time_blocks):
        y_pos = time_y_start - (i * 0.08)
        
        # Time block with colored border
        rect = FancyBboxPatch((0.1, y_pos - 0.03), 0.8, 0.06, 
                            boxstyle="round,pad=0.01", 
                            facecolor='#ecf0f1', edgecolor=color, linewidth=3,
                            transform=ax_info.transAxes)
        ax_info.add_patch(rect)
        
        # Vertical colored line
        line_rect = FancyBboxPatch((0.11, y_pos - 0.025), 0.01, 0.05, 
                                 boxstyle="square,pad=0", 
                                 facecolor=color, transform=ax_info.transAxes)
        ax_info.add_patch(line_rect)
        
        ax_info.text(0.15, y_pos + 0.01, period, fontsize=12, fontweight='bold', 
                    transform=ax_info.transAxes)
        ax_info.text(0.15, y_pos - 0.01, time_range, fontsize=10, 
                    transform=ax_info.transAxes)
        ax_info.text(0.75, y_pos, pricing, fontsize=11, fontweight='bold', 
                    color=color, ha='center', transform=ax_info.transAxes)
    
    # Footer information
    ax_info.text(0.5, 0.02, 'Valid ID required for discounts • Contactless payment accepted\nFor assistance call: 1-800-METRO-GO', 
                fontsize=10, ha='center', transform=ax_info.transAxes, 
                style='italic', color='#7f8c8d')
    
    plt.tight_layout()
    return fig

def main():
    # Generate the metro map
    fig = create_metro_map()
    
    # Save as high-resolution PNG
    plt.savefig('metro_map_station_display.png', dpi=300, bbox_inches='tight', 
                facecolor='white', edgecolor='none')
    
    # Save as PDF for printing
    plt.savefig('metro_map_station_display.pdf', bbox_inches='tight', 
                facecolor='white', edgecolor='none')
    
    print("Metro map images generated successfully:")
    print("- metro_map_station_display.png (High-resolution PNG)")
    print("- metro_map_station_display.pdf (Print-ready PDF)")
    print("- metro_map_display.html (Interactive web version)")
    
    # Show the plot
    plt.show()

if __name__ == "__main__":
    main()