import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import os
from pathlib import Path

class UXFTrackerPlotter:
    def __init__(self, data_directory, output_directory=None):
        self.data_directory = Path(data_directory)
        
        # Set output directory - default to 'plots' subfolder in data directory
        if output_directory is None:
            self.output_directory = self.data_directory / "plots"
        else:
            self.output_directory = Path(output_directory)
        
        # Create output directory if it doesn't exist
        self.output_directory.mkdir(parents=True, exist_ok=True)
        print(f"Plots will be saved to: {self.output_directory}")
        
    def get_all_csv_files(self):
        """Get all CSV files from the specified directory"""
        csv_files = list(self.data_directory.glob("*.csv"))
        return csv_files
    
    def save_plot(self, filename, dpi=300, format='png'):
        """Save the current plot to file"""
        output_path = self.output_directory / f"{filename}.{format}"
        plt.savefig(output_path, dpi=dpi, bbox_inches='tight', format=format)
        print(f"Plot saved: {output_path}")
    
    def load_tracker_data(self, file_path):
        """Load tracker CSV data"""
        try:
            df = pd.read_csv(file_path)
            print(f"Loaded {len(df)} rows from {file_path.name}")
            print(f"Columns: {list(df.columns)}")
            return df
        except Exception as e:
            print(f"Error loading {file_path}: {e}")
            return None
    
    
    def plot_2d_trajectory(self, df, title="2D Trajectory (XZ Plane)", save_filename=None, 
                        background_image=None, world_coordinates=None):
        """Plot 2D trajectory in XZ plane (top-down view) with optional background image"""
        
        if 'pos_x' not in df.columns or 'pos_z' not in df.columns:
            print("Columns 'pos_x' or 'pos_z' not found in data")
            return
        
        # Handle background image plotting
        if background_image is not None:
            try:
                import matplotlib.image as mpimg
                
                # Load image for matplotlib
                img = mpimg.imread(background_image)

                # DEBUG: Print data ranges and world coordinates
                print(f"Image shape: {img.shape}")
                print(f"Data X range: {df['pos_x'].min():.2f} to {df['pos_x'].max():.2f}")
                print(f"Data Z range: {df['pos_z'].min():.2f} to {df['pos_z'].max():.2f}")
                print(f"World coordinates provided: {world_coordinates}")
                
                # Calculate exact figure size to match image aspect ratio
                img_height, img_width = img.shape[:2]
                aspect_ratio = img_width / img_height
                
                # Set figure size
                base_size = 8  # Keep consistent with original size
                if aspect_ratio > 1:  # Wide image
                    fig_width = base_size
                    fig_height = base_size / aspect_ratio
                else:  # Tall image
                    fig_height = base_size
                    fig_width = base_size * aspect_ratio
                
                # Create figure with exact dimensions
                fig, ax = plt.subplots(figsize=(fig_width, fig_height))
                
                # Use provided world coordinates or calculate from data
                if world_coordinates is None:
                    x_min, x_max = df['pos_x'].min(), df['pos_x'].max()
                    z_min, z_max = df['pos_z'].min(), df['pos_z'].max()
                    # Add small padding
                    x_padding = (x_max - x_min) * 0.05
                    z_padding = (z_max - z_min) * 0.05
                    world_coordinates = [x_min - x_padding, x_max + x_padding, 
                                    z_min - z_padding, z_max + z_padding]
                    print(f"Calculated world coordinates from data: {world_coordinates}")
                
                # Check if data is within world coordinate bounds
                data_in_bounds_x = (df['pos_x'] >= world_coordinates[0]) & (df['pos_x'] <= world_coordinates[1])
                data_in_bounds_z = (df['pos_z'] >= world_coordinates[2]) & (df['pos_z'] <= world_coordinates[3])
                data_in_bounds = data_in_bounds_x & data_in_bounds_z

                print(f"Data points within world bounds: {data_in_bounds.sum()} / {len(df)}")
                if data_in_bounds.sum() == 0:
                    print("WARNING: NO data points are within the world coordinate bounds!")
                    print("Consider adjusting your world_coordinates or check your data coordinate system.")
                

                # Display image with world coordinates - LOW ZORDER for background
                ax.imshow(img, extent=world_coordinates, aspect='equal', origin='upper', 
                        alpha=0.8, zorder=1)
                
                # # Plot trajectory with high visibility colors for overlay
                # scatter = ax.scatter(df['pos_x'], df['pos_z'], c=df.index, cmap='plasma', 
                #                 alpha=0.8, s=30, edgecolors='black', linewidth=0.5, zorder=15)
                # ax.plot(df['pos_x'], df['pos_z'], alpha=0.9, linewidth=3, color='yellow', zorder=10)

                scatter = ax.scatter(df['pos_x'], df['pos_z'], c=df.index, cmap='viridis', alpha=0.6, s=20, zorder=15)
                ax.plot(df['pos_x'], df['pos_z'], alpha=0.3, linewidth=1, color='gray', zorder=10)
                
                # Add start and end points with high visibility
                ax.scatter(df['pos_x'].iloc[0], df['pos_z'].iloc[0], 
                        color='green', s=100, marker='o', label='Start', zorder=20,
                        edgecolors='black', linewidth=2)
                ax.scatter(df['pos_x'].iloc[-1], df['pos_z'].iloc[-1], 
                        color='red', s=100, marker='X', label='End', zorder=20,
                        edgecolors='black', linewidth=2)
                
                # Calculate combined bounds
                data_x_min, data_x_max = df['pos_x'].min(), df['pos_x'].max()
                data_z_min, data_z_max = df['pos_z'].min(), df['pos_z'].max()
                
                # Expand limits to include both world coordinates and data
                combined_x_min = min(world_coordinates[0], data_x_min)
                combined_x_max = max(world_coordinates[1], data_x_max)
                combined_z_min = min(world_coordinates[2], data_z_min)
                combined_z_max = max(world_coordinates[3], data_z_max)
                
                # Add some padding
                x_range = combined_x_max - combined_x_min
                z_range = combined_z_max - combined_z_min
                padding_x = x_range * 0.05
                padding_z = z_range * 0.05
                
                ax.set_xlim(combined_x_min - padding_x, combined_x_max + padding_x)
                ax.set_ylim(combined_z_min - padding_z, combined_z_max + padding_z)
                
                print(f"Final plot limits: X[{combined_x_min - padding_x:.2f}, {combined_x_max + padding_x:.2f}], Z[{combined_z_min - padding_z:.2f}, {combined_z_max + padding_z:.2f}]")
                
                # # Set exact limits to match world coordinates
                # ax.set_xlim(world_coordinates[0], world_coordinates[1])
                # ax.set_ylim(world_coordinates[2], world_coordinates[3])
                

                # Labels and styling
                ax.set_xlabel('X Position (m)', fontweight='bold')
                ax.set_ylabel('Z Position (m)', fontweight='bold')
                ax.set_title(title, fontweight='bold')
                
                # Add colorbar
                cbar = plt.colorbar(scatter, ax=ax, label='Time Step', shrink=0.8)
                cbar.set_label('Time Step', fontweight='bold')
                
                # Legend with high zorder
                legend = ax.legend(loc='upper right', framealpha=0.9)
                legend.set_zorder(25)

                 # Add statistics box
                distance_traveled = self.calculate_distance_traveled(df)
                stats_text = f'Distance: {distance_traveled:.2f}m'
                if 'time' in df.columns:
                    duration = df['time'].max() - df['time'].min()
                    avg_speed = distance_traveled / duration if duration > 0 else 0
                    stats_text += f'\nDuration: {duration:.1f}s\nAvg Speed: {avg_speed:.2f}m/s'
                
                ax.text(0.02, 0.98, stats_text, transform=ax.transAxes, 
                        verticalalignment='top', fontsize=10,
                        bbox=dict(boxstyle='round,pad=0.5', facecolor='white', 
                                alpha=0.9, edgecolor='black'), zorder=25)
                
                plt.tight_layout()
                
            except Exception as e:
                print(f"Error loading background image: {e}")
                # Fallback to regular plotting without background
                background_image = None
        
        # Regular plotting without background image (original functionality)
        if background_image is None:
            plt.figure(figsize=(8, 8))
            
            # Create scatter plot with color gradient based on time
            scatter = plt.scatter(df['pos_x'], df['pos_z'], c=df.index, cmap='viridis', alpha=0.6, s=20)
            plt.plot(df['pos_x'], df['pos_z'], alpha=0.3, linewidth=1, color='gray')
            
            # Add start and end points
            plt.scatter(df['pos_x'].iloc[0], df['pos_z'].iloc[0], 
                    color='green', s=100, marker='o', label='Start', zorder=5)
            plt.scatter(df['pos_x'].iloc[-1], df['pos_z'].iloc[-1], 
                    color='red', s=100, marker='X', label='End', zorder=5)
            
            plt.xlabel('X Position (m)')
            plt.ylabel('Z Position (m)')
            plt.title(title)
            plt.colorbar(scatter, label='Time Step')
            plt.legend()
            plt.grid(True, alpha=0.3)
            plt.axis('equal')

            # Add statistics box for non-background mode
            distance_traveled = self.calculate_distance_traveled(df)
            stats_text = f'Distance: {distance_traveled:.2f}m'
            if 'time' in df.columns:
                duration = df['time'].max() - df['time'].min()
                avg_speed = distance_traveled / duration if duration > 0 else 0
                stats_text += f'\nDuration: {duration:.1f}s\nAvg Speed: {avg_speed:.2f}m/s'
            
            plt.text(0.02, 0.98, stats_text, transform=plt.gca().transAxes, 
                    verticalalignment='top',
                    bbox=dict(boxstyle='round', facecolor='white', alpha=0.8))

        plt.tight_layout()

        # Save plot if filename provided
        if save_filename:
            self.save_plot(save_filename)

        # Close the figure to prevent display and free memory
        plt.close()

    def calculate_distance_traveled(self, df):
        """Calculate total distance traveled in XZ plane"""
        if 'pos_x' in df.columns and 'pos_z' in df.columns:
            dx = df['pos_x'].diff()
            dz = df['pos_z'].diff()
            distances = np.sqrt(dx**2 + dz**2)
            return distances.sum()
        return 0
    
    def plot_time_series(self, df, title="Position Over Time", save_filename=None):
        """Plot XZ position over time"""
        plt.figure(figsize=(12, 8))
        
        # Use index as time if time column doesn't exist
        if 'time' in df.columns:
            time_data = df['time']
            time_label = 'Time (s)'
        else:
            time_data = df.index
            time_label = 'Frame'
        
        # Plot X and Z positions
        if 'pos_x' in df.columns:
            plt.plot(time_data, df['pos_x'], label='X Position', alpha=0.7, linewidth=2)
        if 'pos_z' in df.columns:
            plt.plot(time_data, df['pos_z'], label='Z Position', alpha=0.7, linewidth=2)
        
        plt.xlabel(time_label)
        plt.ylabel('Position (m)')
        plt.title(title)
        plt.legend()
        plt.grid(True, alpha=0.3)
        plt.tight_layout()
        
        # Save plot if filename provided
        if save_filename:
            self.save_plot(save_filename)
            
        # plt.show()
    
    def plot_speed_over_time(self, df, title="Speed Over Time", save_filename=None):
        """Plot movement speed in XZ plane over time"""
        plt.figure(figsize=(12, 6))
        
        if 'pos_x' in df.columns and 'pos_z' in df.columns:
            # Calculate speed
            dx = df['pos_x'].diff()
            dz = df['pos_z'].diff()
            
            if 'time' in df.columns:
                dt = df['time'].diff()
                speed = np.sqrt(dx**2 + dz**2) / dt
                time_data = df['time']
                time_label = 'Time (s)'
                speed_label = 'Speed (m/s)'
            else:
                speed = np.sqrt(dx**2 + dz**2)
                time_data = df.index
                time_label = 'Frame'
                speed_label = 'Speed (m/frame)'
            
            # Remove NaN values
            valid_indices = ~np.isnan(speed)
            
            plt.plot(time_data[valid_indices], speed[valid_indices], 
                    linewidth=2, color='blue', alpha=0.7)
            
            plt.xlabel(time_label)
            plt.ylabel(speed_label)
            plt.title(title)
            plt.grid(True, alpha=0.3)
            
            # Add average speed
            avg_speed = speed[valid_indices].mean()
            plt.axhline(y=avg_speed, color='red', linestyle='--', alpha=0.7, 
                       label=f'Average: {avg_speed:.2f}')
            plt.legend()
        else:
            print("Columns 'pos_x' or 'pos_z' not found in data")
            
        plt.tight_layout()
        
        # Save plot if filename provided
        if save_filename:
            self.save_plot(save_filename)
            
        # plt.show()
    
    def plot_heat_map(self, df, title="Position Heat Map (XZ Plane)", bins=50, save_filename=None):
        """Plot heat map of position density in XZ plane"""
        plt.figure(figsize=(10, 8))
        
        if 'pos_x' in df.columns and 'pos_z' in df.columns:
            plt.hist2d(df['pos_x'], df['pos_z'], bins=bins, cmap='hot', alpha=0.8)
            plt.colorbar(label='Density')
            plt.xlabel('X Position (m)')
            plt.ylabel('Z Position (m)')
            plt.title(title)
            plt.axis('equal')
        else:
            print("Columns 'pos_x' or 'pos_z' not found in data")
            
        plt.tight_layout()
        
        # Save plot if filename provided
        if save_filename:
            self.save_plot(save_filename)
            
        # plt.show()
    
    def create_summary_plot(self, df, title_base, save_filename=None):
        """Create a 2x2 summary plot with all visualizations"""
        fig, axes = plt.subplots(2, 2, figsize=(16, 12))
        fig.suptitle(f"{title_base} - Summary", fontsize=16, fontweight='bold')
        
        if 'pos_x' in df.columns and 'pos_z' in df.columns:
            # 1. Trajectory plot
            ax1 = axes[0, 0]
            scatter = ax1.scatter(df['pos_x'], df['pos_z'], c=df.index, cmap='viridis', alpha=0.6, s=15)
            ax1.plot(df['pos_x'], df['pos_z'], alpha=0.3, linewidth=1, color='gray')
            ax1.scatter(df['pos_x'].iloc[0], df['pos_z'].iloc[0], color='green', s=80, marker='o', label='Start')
            ax1.scatter(df['pos_x'].iloc[-1], df['pos_z'].iloc[-1], color='red', s=80, marker='X', label='End')
            ax1.set_xlabel('X Position (m)')
            ax1.set_ylabel('Z Position (m)')
            ax1.set_title('2D Trajectory')
            ax1.legend()
            ax1.grid(True, alpha=0.3)
            ax1.axis('equal')
            
            # 2. Time series
            ax2 = axes[0, 1]
            time_data = df['time'] if 'time' in df.columns else df.index
            ax2.plot(time_data, df['pos_x'], label='X Position', alpha=0.7, linewidth=2)
            ax2.plot(time_data, df['pos_z'], label='Z Position', alpha=0.7, linewidth=2)
            ax2.set_xlabel('Time (s)' if 'time' in df.columns else 'Frame')
            ax2.set_ylabel('Position (m)')
            ax2.set_title('Position Over Time')
            ax2.legend()
            ax2.grid(True, alpha=0.3)
            
            # 3. Speed plot
            ax3 = axes[1, 0]
            dx = df['pos_x'].diff()
            dz = df['pos_z'].diff()
            if 'time' in df.columns:
                dt = df['time'].diff()
                speed = np.sqrt(dx**2 + dz**2) / dt
                time_label = 'Time (s)'
                speed_label = 'Speed (m/s)'
            else:
                speed = np.sqrt(dx**2 + dz**2)
                time_label = 'Frame'
                speed_label = 'Speed (m/frame)'
            
            valid_indices = ~np.isnan(speed)
            ax3.plot(time_data[valid_indices], speed[valid_indices], linewidth=2, color='blue', alpha=0.7)
            avg_speed = speed[valid_indices].mean()
            ax3.axhline(y=avg_speed, color='red', linestyle='--', alpha=0.7, label=f'Avg: {avg_speed:.2f}')
            ax3.set_xlabel(time_label)
            ax3.set_ylabel(speed_label)
            ax3.set_title('Speed Over Time')
            ax3.legend()
            ax3.grid(True, alpha=0.3)
            
            # 4. Heat map
            ax4 = axes[1, 1]
            hist, xedges, yedges = np.histogram2d(df['pos_x'], df['pos_z'], bins=30)
            im = ax4.imshow(hist.T, origin='lower', extent=[xedges[0], xedges[-1], yedges[0], yedges[-1]], 
                           cmap='hot', alpha=0.8, aspect='equal')
            ax4.set_xlabel('X Position (m)')
            ax4.set_ylabel('Z Position (m)')
            ax4.set_title('Position Heat Map')
            plt.colorbar(im, ax=ax4, label='Density')
        
        plt.tight_layout()
        
        # Save plot if filename provided
        if save_filename:
            self.save_plot(save_filename)
            
        # plt.show()
    
    def analyze_single_file(self, file_path, save_plots=True, background_image=None, world_coordinates=None):
        """Analyze a single CSV file with optional background image"""
        df = self.load_tracker_data(file_path)
        
        if df is not None and len(df) > 0:
            title_base = file_path.stem
            
            if save_plots:
                # Plot 2D trajectory (with or without background)
                self.plot_2d_trajectory(df, f"{title_base} - 2D Trajectory", 
                                    save_filename=f"{title_base}_trajectory",
                                    background_image=background_image,
                                    world_coordinates=world_coordinates)
                
                # # Plot time series
                # self.plot_time_series(df, f"{title_base} - Position Over Time", 
                #                      save_filename=f"{title_base}_timeseries")
                
                # # Plot speed
                # self.plot_speed_over_time(df, f"{title_base} - Speed Over Time", 
                #                         save_filename=f"{title_base}_speed")
                
                # # Plot heat map
                # self.plot_heat_map(df, f"{title_base} - Position Heat Map", 
                #                   save_filename=f"{title_base}_heatmap")
                
                # # Create summary plot
                # self.create_summary_plot(df, title_base, save_filename=f"{title_base}_summary")
            else:
                # Plot without saving (with or without background)
                self.plot_2d_trajectory(df, f"{title_base} - 2D Trajectory",
                                    background_image=background_image,
                                    world_coordinates=world_coordinates)
                # self.plot_time_series(df, f"{title_base} - Position Over Time")
                # self.plot_speed_over_time(df, f"{title_base} - Speed Over Time")
                # self.plot_heat_map(df, f"{title_base} - Position Heat Map")
                # self.create_summary_plot(df, title_base)
            
            # Print statistics
            self.print_statistics(df, title_base)
    
    def analyze_all_files(self, save_plots=True, background_image=None, world_coordinates=None):
        """Analyze all CSV files in the directory with optional background image"""
        csv_files = self.get_all_csv_files()
        
        if not csv_files:
            print("No CSV files found in the directory!")
            return
        
        print(f"Found {len(csv_files)} CSV files:")
        for file_path in csv_files:
            print(f"  - {file_path.name}")
        
        for file_path in csv_files:
            print(f"\n{'='*50}")
            print(f"Analyzing: {file_path.name}")
            print(f"{'='*50}")
            self.analyze_single_file(file_path, save_plots=save_plots, 
                                background_image=background_image,
                                world_coordinates=world_coordinates)
    
    def print_statistics(self, df, title):
        """Print movement statistics"""
        if 'pos_x' in df.columns and 'pos_z' in df.columns:
            print(f"\nStatistics for {title}:")
            print(f"  Total data points: {len(df)}")
            print(f"  X range: {df['pos_x'].min():.2f} to {df['pos_x'].max():.2f}")
            print(f"  Z range: {df['pos_z'].min():.2f} to {df['pos_z'].max():.2f}")
            print(f"  Distance traveled: {self.calculate_distance_traveled(df):.2f}m")
            
            if 'time' in df.columns:
                duration = df['time'].max() - df['time'].min()
                print(f"  Duration: {duration:.2f}s")
                avg_speed = self.calculate_distance_traveled(df) / duration if duration > 0 else 0
                print(f"  Average speed: {avg_speed:.2f}m/s")

    def plot_2d_trajectory_overlay(self, df, title="2D Trajectory Overlay", save_filename=None, 
                              background_image=None, image_extent=None, remove_axes=False):
        """Plot 2D trajectory overlaid on background image"""
        
        if background_image is None:
            print("Background image is required for overlay")
            return
        
        try:
            import matplotlib.image as mpimg
            img = mpimg.imread(background_image)
            
            # Get image dimensions
            img_height, img_width = img.shape[:2]
            aspect_ratio = img_width / img_height
            
            # Create figure with exact aspect ratio of image
            fig_height = 8  # Base height in inches
            fig_width = fig_height * aspect_ratio
            
            plt.figure(figsize=(fig_width, fig_height))
            
            # If extent not provided, calculate from data
            if image_extent is None:
                if 'pos_x' in df.columns and 'pos_z' in df.columns:
                    x_min, x_max = df['pos_x'].min(), df['pos_x'].max()
                    z_min, z_max = df['pos_z'].min(), df['pos_z'].max()
                    # Add small padding
                    x_padding = (x_max - x_min) * 0.05
                    z_padding = (z_max - z_min) * 0.05
                    image_extent = [x_min - x_padding, x_max + x_padding, 
                                z_min - z_padding, z_max + z_padding]
                else:
                    print("No position data found")
                    return
            
            # Display image with exact extent
            # plt.imshow(img, extent=image_extent, aspect='equal', interpolation='bilinear')
            
            # Plot trajectory data
            if 'pos_x' in df.columns and 'pos_z' in df.columns:
                # Plot trajectory line
                plt.plot(df['pos_x'], df['pos_z'], alpha=0.8, linewidth=3, 
                        color='yellow', label='Path')
                
                # Plot trajectory points with time color coding
                scatter = plt.scatter(df['pos_x'], df['pos_z'], c=df.index, 
                                    cmap='plasma', alpha=0.7, s=25, 
                                    edgecolors='white', linewidth=0.5)
                
                # Add start and end points
                plt.scatter(df['pos_x'].iloc[0], df['pos_z'].iloc[0], 
                        color='lime', s=150, marker='o', label='Start', 
                        zorder=10, edgecolors='black', linewidth=2)
                plt.scatter(df['pos_x'].iloc[-1], df['pos_z'].iloc[-1], 
                        color='red', s=150, marker='X', label='End', 
                        zorder=10, edgecolors='black', linewidth=2)
                
                # Set exact limits to match image
                plt.xlim(image_extent[0], image_extent[1])
                plt.ylim(image_extent[2], image_extent[3])
                
                # Configure axes
                if remove_axes:
                    plt.axis('off')
                else:
                    plt.xlabel('X Position (m)', fontsize=12, fontweight='bold')
                    plt.ylabel('Z Position (m)', fontsize=12, fontweight='bold')
                    plt.title(title, fontsize=14, fontweight='bold')
                    
                    # Add colorbar
                    cbar = plt.colorbar(scatter, label='Time Step', shrink=0.8)
                    cbar.ax.tick_params(labelsize=10)
                    
                    # Add legend
                    plt.legend(loc='upper right', fontsize=10, 
                            fancybox=True, shadow=True, framealpha=0.9)
                
                # Remove any padding/margins for perfect fit
                plt.subplots_adjust(left=0, right=1, top=1, bottom=0)
                plt.margins(0)
                
                # Add statistics box
                if not remove_axes:
                    distance_traveled = self.calculate_distance_traveled(df)
                    stats_text = f'Distance: {distance_traveled:.2f}m'
                    if 'time' in df.columns:
                        duration = df['time'].max() - df['time'].min()
                        avg_speed = distance_traveled / duration if duration > 0 else 0
                        stats_text += f'\nDuration: {duration:.1f}s\nAvg Speed: {avg_speed:.2f}m/s'
                    
                    plt.text(0.02, 0.98, stats_text, transform=plt.gca().transAxes, 
                            verticalalignment='top', fontsize=10,
                            bbox=dict(boxstyle='round,pad=0.5', facecolor='white', 
                                    alpha=0.9, edgecolor='black'))
            
            # Save plot if filename provided
            if save_filename:
                # Save with high DPI and no padding
                output_path = self.output_directory / f"{save_filename}.png"
                plt.savefig(output_path, dpi=300, bbox_inches='tight', 
                        pad_inches=0, facecolor='white', edgecolor='none')
                print(f"Overlay plot saved: {output_path}")
            
            # plt.show()
            
        except Exception as e:
            print(f"Error creating overlay: {e}")

    def create_perfect_overlay_from_image_dimensions(self, df, background_image, 
                                        world_coordinates, title="Path Trace",
                                        save_filename=None):
        """Create overlay with exact image dimensions and world coordinates"""
        
        try:
            import matplotlib.image as mpimg
            from PIL import Image
            
            # Load image for matplotlib
            img = mpimg.imread(background_image)
            
            # Calculate exact figure size to match image aspect ratio
            img_height, img_width = img.shape[:2]
            aspect_ratio = img_width / img_height
            
            # Set figure size (you can adjust base_size as needed)
            base_size = 12  # inches - increased for better visibility
            if aspect_ratio > 1:  # Wide image
                fig_width = base_size
                fig_height = base_size / aspect_ratio
            else:  # Tall image
                fig_height = base_size
                fig_width = base_size * aspect_ratio
            
            # Create figure with exact dimensions
            plt.figure(figsize=(fig_width, fig_height))
            
            # Display image with world coordinates
            # world_coordinates = [x_min, x_max, z_min, z_max]
            plt.imshow(img, extent=world_coordinates, aspect='equal', origin='lower')
            
            # Plot trajectory
            if 'pos_x' in df.columns and 'pos_z' in df.columns:
                # Plot path with high visibility
                plt.plot(df['pos_x'], df['pos_z'], color='yellow', linewidth=4, 
                    alpha=0.9, label='Path', zorder=10)
                
                # Plot points with time color coding
                scatter = plt.scatter(df['pos_x'], df['pos_z'], c=df.index, 
                                cmap='plasma', s=30, alpha=0.8,
                                edgecolors='white', linewidth=0.5, zorder=15)
                
                # Start and end points with high visibility
                plt.scatter(df['pos_x'].iloc[0], df['pos_z'].iloc[0], 
                        color='lime', s=150, marker='o', label='Start', 
                        zorder=20, edgecolors='black', linewidth=2)
                plt.scatter(df['pos_x'].iloc[-1], df['pos_z'].iloc[-1], 
                        color='red', s=150, marker='X', label='End', 
                        zorder=200, edgecolors='black', linewidth=2)
                
                # Set exact limits to match world coordinates
                plt.xlim(world_coordinates[0], world_coordinates[1])
                plt.ylim(world_coordinates[2], world_coordinates[3])
                
                # Labels and title
                plt.xlabel('X Position (m)', fontweight='bold', fontsize=12)
                plt.ylabel('Z Position (m)', fontweight='bold', fontsize=12)
                plt.title(title, fontweight='bold', fontsize=14, pad=20)
                
                # Add colorbar
                cbar = plt.colorbar(scatter, shrink=0.8)
                cbar.set_label('Time Step', fontweight='bold')
                
                # Legend with better positioning
                legend = plt.legend(loc='upper right', framealpha=0.9, fontsize=10)
                legend.set_zorder(25)

                # Add statistics box
                distance_traveled = self.calculate_distance_traveled(df)
                stats_text = f'Distance: {distance_traveled:.2f}m'
                if 'time' in df.columns:
                    duration = df['time'].max() - df['time'].min()
                    avg_speed = distance_traveled / duration if duration > 0 else 0
                    stats_text += f'\nDuration: {duration:.1f}s\nAvg Speed: {avg_speed:.2f}m/s'
                
                plt.text(0.02, 0.98, stats_text, transform=plt.gca().transAxes, 
                        verticalalignment='top', fontsize=10,
                        bbox=dict(boxstyle='round,pad=0.5', facecolor='white', 
                                alpha=0.9, edgecolor='black'), zorder=25)
            
            plt.tight_layout()
            
            # Save with high DPI
            if save_filename:
                output_path = self.output_directory / f"{save_filename}.png"
                plt.savefig(output_path, dpi=300, bbox_inches='tight', 
                        facecolor='white', edgecolor='none')
                print(f"Overlay plot saved: {output_path}")
            
            # Close the figure to prevent display and free memory
            plt.close()
            
        except Exception as e:
            print(f"Error creating overlaid plot: {e}")
            import traceback
            traceback.print_exc()

    # Analyze single file with overlay
    def analyze_single_file_with_overlay(self, file_path, background_image, 
                                    world_coordinates, save_plots=True):
        """Analyze single file with overlaid image"""
        df = self.load_tracker_data(file_path)
        
        if df is not None and len(df) > 0:
            title_base = file_path.stem
            
            if save_plots:
                # # Create perfect overlay
                # self.create_perfect_overlay_from_image_dimensions(
                #     df, background_image, world_coordinates,
                #     title=f"{title_base} - Path Trace",
                #     save_filename=f"{title_base}_path_trace"
                # )
                self.plot_2d_trajectory_overlay(
                    df, f"{title_base} - 2D Trajectory Overlay",
                    save_filename=f"{title_base}_path_trace",
                    background_image=background_image,
                    image_extent=world_coordinates
                )

                # Regular analysis
                self.analyze_single_file(file_path, save_plots=True)
            
            self.print_statistics(df, title_base)

    # Analyze all files with overlay
    def analyze_all_files_with_overlay(self, background_image, world_coordinates, save_plots=True):
        """
        Analyze all CSV files in the directory with background image overlay.
        This method processes all CSV files found in the specified directory, applying
        background image overlay analysis to each file. Each file's plots are saved
        in a separate subfolder named after the file.
        
        Args:
            background_image: The background image to overlay on the analysis plots
            world_coordinates: Coordinate system mapping for the world space
            save_plots (bool, optional): Whether to save the generated plots to disk.
                                       Defaults to True.
        Returns:
            None
        Prints:
            - Number of CSV files found
            - List of all CSV file names
            - Progress indicators for each file being analyzed
            - Warning message if no CSV files are found
        Note:
            If no CSV files are found in the directory, the method will print a warning
            message and return early without processing.
        """
        csv_files = self.get_all_csv_files()
        
        if not csv_files:
            print("No CSV files found in the directory!")
            return
        
        print(f"Found {len(csv_files)} CSV files:")
        for file_path in csv_files:
            print(f"  - {file_path.name}")
        
        # Store original output directory
        original_output_dir = self.output_directory
        
        for file_path in csv_files:
            print(f"\n{'='*50}")
            print(f"Analyzing with overlay: {file_path.name}")
            print(f"{'='*50}")
            
            # Create file-specific output directory
            file_specific_dir = original_output_dir / file_path.stem
            file_specific_dir.mkdir(parents=True, exist_ok=True)
            
            # Temporarily change output directory
            self.output_directory = file_specific_dir
            print(f"Plots for {file_path.name} will be saved to: {file_specific_dir}")
            
            self.analyze_single_file_with_overlay(
                file_path, background_image, world_coordinates, save_plots=save_plots
            )
        
        # Restore original output directory
        self.output_directory = original_output_dir

# Usage example
def main():
    # Set your data directory path
    participant_id = input("Enter participant ID (same ID as folder name for participant): ")
    session_id = input("Enter session ID (e.g., S001): ")
    base_data_path = r"C:\Users\cogni\Documents\vr-experiment-data\vr_locomotion"
    data_directory = os.path.join(base_data_path, participant_id, session_id, "trackers")
    background_image = r"C:\Users\cogni\Documents\vr-experiment-data\map.png"
    
    # Optional: specify custom output directory
    output_directory = r"C:\Users\cogni\Documents\vr-experiment-data\data_plots"
    participant_results = os.path.join(output_directory, participant_id, session_id)

    # Define world coordinates that match your image
    # [x_min, x_max, z_min, z_max] in Unity world units
    world_coordinates = [-111, -89, -10.5, 11.5]  # Adjust to your environment
    
    # Create plotter instance
    plotter = UXFTrackerPlotter(data_directory, participant_results)
    
    # Analyze all CSV files and save plots
    # plotter.analyze_all_files(save_plots=True)
    
    # Or analyze without saving plots
    # plotter.analyze_all_files(save_plots=False)
    
    # # Or analyze a specific file
    # data_file = Path(data_directory) / "locomotion_movement_T001.csv"
    # if data_file.exists():
    #     plotter.analyze_single_file(
    #         data_file,
    #         background_image=background_image,
    #         world_coordinates=world_coordinates,
    #         save_plots=True
    #     )

    # Analyze all files with overlay (if needed)
    plotter.analyze_all_files(
        background_image=background_image,
        world_coordinates=world_coordinates,
        save_plots=True
    )

if __name__ == "__main__":
    main()