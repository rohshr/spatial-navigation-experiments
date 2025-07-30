import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
import os
from pathlib import Path
import re

class UXFTrackerPlotter:
    def __init__(self, participant_folder, background_image, world_coordinates, output_directory=None):
        self.participant_folder = Path(participant_folder)
        self.background_image = Path(background_image)
        self.world_coordinates = world_coordinates
        
        # Set output directory - default to 'analysis_plots' in participant folder
        if output_directory is None:
            self.output_directory = self.participant_folder / "analysis_plots"
        else:
            self.output_directory = Path(output_directory)
        
        # Create output directory if it doesn't exist
        self.output_directory.mkdir(parents=True, exist_ok=True)
        print(f"Plots will be saved to: {self.output_directory}")
        
        # Get participant name from folder
        self.participant_name = self.participant_folder.name
        
    def get_session_folders(self):
        """Get all session folders from participant directory"""
        session_folders = [f for f in self.participant_folder.iterdir() 
                          if f.is_dir() and f.name.startswith('S')]
        return sorted(session_folders)
    
    def get_tracker_files(self, session_folder):
        """Get all tracker CSV files from a session's trackers folder"""
        trackers_folder = session_folder / "trackers"
        if not trackers_folder.exists():
            return []
        
        csv_files = list(trackers_folder.glob("locomotion_movement_T*.csv"))
        return sorted(csv_files)
    
    def extract_trial_number(self, filename):
        """Extract trial number from filename"""
        match = re.search(r'T(\d+)', filename)
        return int(match.group(1)) if match else 0
    
    def load_tracker_data(self, file_path):
        """Load tracker CSV data"""
        try:
            df = pd.read_csv(file_path)
            print(f"Loaded {len(df)} rows from {file_path.name}")
            return df
        except Exception as e:
            print(f"Error loading {file_path}: {e}")
            return None
    
    def calculate_distance_traveled(self, df):
        """Calculate total distance traveled in XZ plane"""
        if 'pos_x' in df.columns and 'pos_z' in df.columns:
            dx = df['pos_x'].diff()
            dz = df['pos_z'].diff()
            distances = np.sqrt(dx**2 + dz**2)
            return distances.sum()
        return 0
    
    def plot_trajectory_overlay(self, df, title="Path Trajectory", save_filename=None):
        """Plot trajectory overlaid on background image"""
        try:
            import matplotlib.image as mpimg
            from PIL import Image
            
            # Get exact image dimensions
            with Image.open(self.background_image) as pil_img:
                img_width_px, img_height_px = pil_img.size
            
            # Load image for matplotlib
            img = mpimg.imread(self.background_image)
            
            # Calculate figure size to match image aspect ratio
            aspect_ratio = img_width_px / img_height_px
            base_size = 10
            if aspect_ratio > 1:
                fig_width = base_size
                fig_height = base_size / aspect_ratio
            else:
                fig_height = base_size
                fig_width = base_size * aspect_ratio
            
            # Create figure
            fig, ax = plt.subplots(figsize=(fig_width, fig_height))
            
            # Display background image
            ax.imshow(img, extent=self.world_coordinates, aspect='equal')
            
            # Plot trajectory
            if 'pos_x' in df.columns and 'pos_z' in df.columns:
                # Plot path line
                ax.plot(df['pos_x'], df['pos_z'], color='yellow', linewidth=3, 
                       alpha=0.8, label='Path')
                
                # Plot points with time color coding
                scatter = ax.scatter(df['pos_x'], df['pos_z'], c=df.index, 
                                   cmap='plasma', s=20, alpha=0.7,
                                   edgecolors='white', linewidth=0.3)
                
                # Start and end points
                ax.scatter(df['pos_x'].iloc[0], df['pos_z'].iloc[0], 
                          color='lime', s=100, marker='o', label='Start', 
                          zorder=10, edgecolors='black', linewidth=1)
                ax.scatter(df['pos_x'].iloc[-1], df['pos_z'].iloc[-1], 
                          color='red', s=100, marker='X', label='End', 
                          zorder=10, edgecolors='black', linewidth=1)
                
                # Set exact limits
                ax.set_xlim(self.world_coordinates[0], self.world_coordinates[1])
                ax.set_ylim(self.world_coordinates[2], self.world_coordinates[3])
                
                # Labels and title
                ax.set_xlabel('X Position (m)', fontweight='bold')
                ax.set_ylabel('Z Position (m)', fontweight='bold')
                ax.set_title(title, fontweight='bold', pad=20)
                
                # Add colorbar
                cbar = plt.colorbar(scatter, ax=ax, shrink=0.8)
                cbar.set_label('Time Step', fontweight='bold')
                
                # Legend
                ax.legend(loc='upper right', framealpha=0.9)
                
                # Add statistics
                distance_traveled = self.calculate_distance_traveled(df)
                stats_text = f'Distance: {distance_traveled:.2f}m'
                if 'time' in df.columns:
                    duration = df['time'].max() - df['time'].min()
                    avg_speed = distance_traveled / duration if duration > 0 else 0
                    stats_text += f'\nDuration: {duration:.1f}s\nAvg Speed: {avg_speed:.2f}m/s'
                
                ax.text(0.02, 0.98, stats_text, transform=ax.transAxes, 
                       verticalalignment='top', fontsize=10,
                       bbox=dict(boxstyle='round,pad=0.5', facecolor='white', 
                               alpha=0.9, edgecolor='black'))
            
            plt.tight_layout()
            
            # Save plot
            if save_filename:
                plt.savefig(save_filename, dpi=300, bbox_inches='tight', 
                           pad_inches=0.1, facecolor='white')
                print(f"Trajectory plot saved: {save_filename}")
            
            plt.close()
            
        except Exception as e:
            print(f"Error creating trajectory overlay: {e}")
    
    def plot_heatmap_overlay(self, df, title="Position Heat Map", save_filename=None):
        """Plot heatmap overlaid on background image"""
        try:
            import matplotlib.image as mpimg
            from PIL import Image
            
            # Get exact image dimensions
            with Image.open(self.background_image) as pil_img:
                img_width_px, img_height_px = pil_img.size
            
            # Load image for matplotlib
            img = mpimg.imread(self.background_image)
            
            # Calculate figure size to match image aspect ratio
            aspect_ratio = img_width_px / img_height_px
            base_size = 10
            if aspect_ratio > 1:
                fig_width = base_size
                fig_height = base_size / aspect_ratio
            else:
                fig_height = base_size
                fig_width = base_size * aspect_ratio
            
            # Create figure
            fig, ax = plt.subplots(figsize=(fig_width, fig_height))
            
            # Display background image
            ax.imshow(img, extent=self.world_coordinates, aspect='equal')
            
            # Plot heatmap
            if 'pos_x' in df.columns and 'pos_z' in df.columns:
                # Create 2D histogram
                hist, xedges, yedges = np.histogram2d(df['pos_x'], df['pos_z'], 
                                                     bins=30, range=[[self.world_coordinates[0], self.world_coordinates[1]],
                                                                   [self.world_coordinates[2], self.world_coordinates[3]]])
                
                # Create heatmap overlay
                im = ax.imshow(hist.T, origin='lower', 
                              extent=[xedges[0], xedges[-1], yedges[0], yedges[-1]], 
                              cmap='hot', alpha=0.6, aspect='equal')
                
                # Set exact limits
                ax.set_xlim(self.world_coordinates[0], self.world_coordinates[1])
                ax.set_ylim(self.world_coordinates[2], self.world_coordinates[3])
                
                # Labels and title
                ax.set_xlabel('X Position (m)', fontweight='bold')
                ax.set_ylabel('Z Position (m)', fontweight='bold')
                ax.set_title(title, fontweight='bold', pad=20)
                
                # Add colorbar
                cbar = plt.colorbar(im, ax=ax, shrink=0.8)
                cbar.set_label('Density', fontweight='bold')
            
            plt.tight_layout()
            
            # Save plot
            if save_filename:
                plt.savefig(save_filename, dpi=300, bbox_inches='tight', 
                           pad_inches=0.1, facecolor='white')
                print(f"Heatmap plot saved: {save_filename}")
            
            plt.close()
            
        except Exception as e:
            print(f"Error creating heatmap overlay: {e}")
    
    def plot_session_summary(self, session_dataframes, trial_numbers, session_name, save_filename=None):
        """Plot all trials from a session on one trajectory plot"""
        try:
            import matplotlib.image as mpimg
            from PIL import Image
            
            # Get exact image dimensions
            with Image.open(self.background_image) as pil_img:
                img_width_px, img_height_px = pil_img.size
            
            # Load image for matplotlib
            img = mpimg.imread(self.background_image)
            
            # Calculate figure size
            aspect_ratio = img_width_px / img_height_px
            base_size = 12
            if aspect_ratio > 1:
                fig_width = base_size
                fig_height = base_size / aspect_ratio
            else:
                fig_height = base_size
                fig_width = base_size * aspect_ratio
            
            # Create figure
            fig, ax = plt.subplots(figsize=(fig_width, fig_height))
            
            # Display background image
            ax.imshow(img, extent=self.world_coordinates, aspect='equal')
            
            # Plot each trial
            colors = plt.cm.tab10(np.linspace(0, 1, len(session_dataframes)))
            
            for i, (df, trial_num) in enumerate(zip(session_dataframes, trial_numbers)):
                if 'pos_x' in df.columns and 'pos_z' in df.columns:
                    color = colors[i]
                    
                    # Plot trajectory
                    ax.plot(df['pos_x'], df['pos_z'], alpha=0.8, linewidth=2, 
                           color=color, label=f'Trial {trial_num}')
                    
                    # Add start point
                    ax.scatter(df['pos_x'].iloc[0], df['pos_z'].iloc[0], 
                              color=color, s=80, marker='o', zorder=5,
                              edgecolors='white', linewidth=1)
                    
                    # Add end point
                    ax.scatter(df['pos_x'].iloc[-1], df['pos_z'].iloc[-1], 
                              color=color, s=80, marker='X', zorder=5,
                              edgecolors='white', linewidth=1)
            
            # Set exact limits
            ax.set_xlim(self.world_coordinates[0], self.world_coordinates[1])
            ax.set_ylim(self.world_coordinates[2], self.world_coordinates[3])
            
            # Labels and title
            ax.set_xlabel('X Position (m)', fontweight='bold')
            ax.set_ylabel('Z Position (m)', fontweight='bold')
            ax.set_title(f'{session_name} - All Trials Summary', fontweight='bold', pad=20)
            
            # Legend
            ax.legend(loc='upper right', framealpha=0.9, fontsize=10)
            
            plt.tight_layout()
            
            # Save plot
            if save_filename:
                plt.savefig(save_filename, dpi=300, bbox_inches='tight', 
                           pad_inches=0.1, facecolor='white')
                print(f"Session summary plot saved: {save_filename}")
            
            plt.close()
            
        except Exception as e:
            print(f"Error creating session summary: {e}")
    
    def analyze_participant(self):
        """Analyze all sessions for the participant"""
        session_folders = self.get_session_folders()
        
        if not session_folders:
            print("No session folders found!")
            return
        
        print(f"Found {len(session_folders)} session folders for {self.participant_name}")
        
        for session_folder in session_folders:
            session_name = session_folder.name
            print(f"\n{'='*60}")
            print(f"Processing {session_name}")
            print(f"{'='*60}")
            
            # Get all tracker files for this session
            tracker_files = self.get_tracker_files(session_folder)
            
            if not tracker_files:
                print(f"No tracker files found in {session_name}")
                continue
            
            # Create session output directory
            session_output = self.output_directory / self.participant_name / session_name
            session_output.mkdir(parents=True, exist_ok=True)
            
            session_dataframes = []
            trial_numbers = []
            
            # Process each trial
            for tracker_file in tracker_files:
                trial_num = self.extract_trial_number(tracker_file.name)
                trial_folder = session_output / f"T{trial_num:03d}"
                trial_folder.mkdir(parents=True, exist_ok=True)
                
                print(f"Processing Trial {trial_num}...")
                
                # Load data
                df = self.load_tracker_data(tracker_file)
                
                if df is not None and len(df) > 0:
                    # Store for session summary
                    session_dataframes.append(df)
                    trial_numbers.append(trial_num)
                    
                    # Create individual trial plots
                    trial_base_name = f"{self.participant_name}_{session_name}_T{trial_num:03d}"
                    
                    # Trajectory plot
                    trajectory_filename = trial_folder / f"{trial_base_name}_trajectory.png"
                    self.plot_trajectory_overlay(
                        df, 
                        title=f"{trial_base_name} - Path Trajectory",
                        save_filename=trajectory_filename
                    )
                    
                    # Heatmap plot
                    heatmap_filename = trial_folder / f"{trial_base_name}_heatmap.png"
                    self.plot_heatmap_overlay(
                        df,
                        title=f"{trial_base_name} - Position Heat Map",
                        save_filename=heatmap_filename
                    )
                    
                    # Print basic statistics
                    distance = self.calculate_distance_traveled(df)
                    print(f"  Distance traveled: {distance:.2f}m")
                    if 'time' in df.columns:
                        duration = df['time'].max() - df['time'].min()
                        avg_speed = distance / duration if duration > 0 else 0
                        print(f"  Duration: {duration:.1f}s, Avg Speed: {avg_speed:.2f}m/s")
            
            # Create session summary plot
            if session_dataframes:
                summary_filename = session_output / f"{self.participant_name}_{session_name}_summary.png"
                self.plot_session_summary(
                    session_dataframes, 
                    trial_numbers, 
                    f"{self.participant_name} - {session_name}",
                    save_filename=summary_filename
                )
                
                print(f"\nSession {session_name} completed: {len(session_dataframes)} trials processed")
        
        print(f"\n{'='*60}")
        print(f"Analysis completed for {self.participant_name}")
        print(f"All plots saved to: {self.output_directory}")
        print(f"{'='*60}")

# Usage example
def main():
    # Set your participant folder path
    participant_folder = r"C:\Users\rohan\Documents\spatial_nav_data\vr_locomotion_settings\ebony-3f1e7031-785e-4b5e-a137-f9aad064cb22"
    
    # Background image path
    background_image = r"C:\Users\rohan\Documents\spatial_nav_data\map.png"
    
    # World coordinates [x_min, x_max, z_min, z_max]
    world_coordinates = [-86.2, -63.8, -10.6, 11.8]
    
    # Optional: specify custom output directory
    output_directory = r"C:\Users\rohan\Documents\spatial_nav_data\analysis_results_3"
    
    # Create plotter instance
    plotter = UXFTrackerPlotter(
        participant_folder=participant_folder,
        background_image=background_image,
        world_coordinates=world_coordinates,
        output_directory=output_directory
    )
    
    # Analyze all sessions and trials
    plotter.analyze_participant()

if __name__ == "__main__":
    main()