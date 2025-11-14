# Load required libraries
library(tidyverse)
library(fs)

# Function to extract trial number from filename
extract_trial_number <- function(filename) {
  # Extract trial number (e.g., T001, T002)
  trial_match <- str_extract(filename, "T\\d+")
  return(trial_match)
}

# Function to load and process all participant data
load_all_participant_data <- function(base_data_path) {
  
  # Get all participant folders
  participant_folders <- dir_ls(base_data_path, type = "directory")
  
  # Initialize list to store trial data
  trial_data_list <- list()
  
  # Loop through each participant
  for (participant_path in participant_folders) {
    participant_id <- basename(participant_path)
    
    # Get all session folders for this participant
    session_folders <- dir_ls(participant_path, type = "directory")
    
    for (session_path in session_folders) {
      session_id <- basename(session_path)
      
      # Get tracker folder path
      tracker_path <- file.path(session_path, "trackers")
      
      # Check if tracker folder exists
      if (dir_exists(tracker_path)) {
        
        # Get all CSV files in tracker folder
        csv_files <- dir_ls(tracker_path, glob = "*.csv")
        
        # Loop through each CSV file
        for (csv_file in csv_files) {
          filename <- basename(csv_file)
          trial_num <- extract_trial_number(filename)
          
          if (!is.na(trial_num)) {
            # Read the CSV file
            tryCatch({
              df <- read_csv(csv_file, show_col_types = FALSE)
              
              # Select only time, pos_x, pos_z columns
              if (all(c("pos_x", "pos_z") %in% names(df))) {
                
                # Create column names with participant and session info
                df_selected <- df %>%
                  select(time = if("time" %in% names(.)) time else NULL,
                         pos_x, pos_z) %>%
                  rename_with(~paste0(participant_id, "_", session_id, "_", .x),
                              .cols = -time)
                
                # If time column doesn't exist, create an index
                if (!"time" %in% names(df)) {
                  df_selected <- df_selected %>%
                    mutate(time = row_number())
                }
                
                # Rename time column to include participant/session info
                df_selected <- df_selected %>%
                  rename(!!paste0(participant_id, "_", session_id, "_time") := time)
                
                # Store in trial list
                if (is.null(trial_data_list[[trial_num]])) {
                  trial_data_list[[trial_num]] <- df_selected
                } else {
                  # Merge with existing data for this trial
                  trial_data_list[[trial_num]] <- bind_cols(
                    trial_data_list[[trial_num]], 
                    df_selected
                  )
                }
                
                message(sprintf("Loaded: %s - %s - %s", 
                                participant_id, session_id, trial_num))
              }
            }, error = function(e) {
              warning(sprintf("Error loading %s: %s", csv_file, e$message))
            })
          }
        }
      }
    }
  }
  
  return(trial_data_list)
}

# Alternative function: Create separate dataframes per trial with participant grouping
load_participant_data_by_trial <- function(base_data_path) {
  
  # Get all participant folders
  participant_folders <- dir_ls(base_data_path, type = "directory")
  
  # Initialize list to store trial data
  trial_data_list <- list()
  
  # Loop through each participant
  for (participant_path in participant_folders) {
    participant_id <- basename(participant_path)
    
    # Get all session folders for this participant
    session_folders <- dir_ls(participant_path, type = "directory")
    
    for (session_path in session_folders) {
      session_id <- basename(session_path)
      
      # Get tracker folder path
      tracker_path <- file.path(session_path, "trackers")
      
      # Check if tracker folder exists
      if (dir_exists(tracker_path)) {
        
        # Get all CSV files in tracker folder
        csv_files <- dir_ls(tracker_path, glob = "*.csv")
        
        # Loop through each CSV file
        for (csv_file in csv_files) {
          filename <- basename(csv_file)
          trial_num <- extract_trial_number(filename)
          
          if (!is.na(trial_num)) {
            # Read the CSV file
            tryCatch({
              df <- read_csv(csv_file, show_col_types = FALSE)
              
              # Select only time, pos_x, pos_z columns and add metadata
              if (all(c("pos_x", "pos_z") %in% names(df))) {
                
                df_selected <- df %>%
                  mutate(
                    participant_id = participant_id,
                    session_id = session_id,
                    row_num = row_number()
                  )
                
                # Use time if available, otherwise use row number
                if ("time" %in% names(df)) {
                  df_selected <- df_selected %>%
                    select(participant_id, session_id, time, pos_x, pos_z, row_num)
                } else {
                  df_selected <- df_selected %>%
                    select(participant_id, session_id, time = row_num, pos_x, pos_z)
                }
                
                # Store in trial list
                if (is.null(trial_data_list[[trial_num]])) {
                  trial_data_list[[trial_num]] <- df_selected
                } else {
                  trial_data_list[[trial_num]] <- bind_rows(
                    trial_data_list[[trial_num]], 
                    df_selected
                  )
                }
                
                message(sprintf("Loaded: %s - %s - %s", 
                                participant_id, session_id, trial_num))
              }
            }, error = function(e) {
              warning(sprintf("Error loading %s: %s", csv_file, e$message))
            })
          }
        }
      }
    }
  }
  
  return(trial_data_list)
}

# Function to create wide format for a specific trial
create_wide_format_trial <- function(trial_df) {
  trial_df %>%
    group_by(participant_id, session_id) %>%
    mutate(row_id = row_number()) %>%
    ungroup() %>%
    pivot_wider(
      id_cols = row_id,
      names_from = c(participant_id, session_id),
      values_from = c(time, pos_x, pos_z),
      names_glue = "{participant_id}_{session_id}_{.value}"
    ) %>%
    select(-row_id)
}

# Main execution
main <- function() {
  # Set base data path
  base_data_path <- "C:/Users/cogni/Documents/vr-experiment-data/test"
  
  # Load all participant data by trial (long format)
  message("Loading all participant data...")
  trial_data_long <- load_participant_data_by_trial(base_data_path)
  
  message(sprintf("\nFound data for %d trials", length(trial_data_long)))
  
  # Print summary for each trial
  for (trial_name in names(trial_data_long)) {
    message(sprintf("\n%s:", trial_name))
    message(sprintf("  Participants: %d", 
                    n_distinct(trial_data_long[[trial_name]]$participant_id)))
    message(sprintf("  Total rows: %d", 
                    nrow(trial_data_long[[trial_name]])))
    
    # Show participant breakdown
    participant_summary <- trial_data_long[[trial_name]] %>%
      group_by(participant_id, session_id) %>%
      summarise(n_rows = n(), .groups = "drop")
    
    print(participant_summary)
  }
  
  # Optional: Create wide format for each trial
  message("\nCreating wide format dataframes...")
  trial_data_wide <- map(trial_data_long, create_wide_format_trial)
  
  # Return both formats
  return(list(
    long_format = trial_data_long,
    wide_format = trial_data_wide
  ))
}

# Helper function to save trial data to CSV
save_trial_data <- function(trial_data_list, output_dir, format = "long") {
  dir.create(output_dir, recursive = TRUE, showWarnings = FALSE)
  
  for (trial_name in names(trial_data_list)) {
    output_file <- file.path(output_dir, paste0(trial_name, "_", format, ".csv"))
    write_csv(trial_data_list[[trial_name]], output_file)
    message(sprintf("Saved: %s", output_file))
  }
}

# # Run the script
# if (!interactive()) {
#   results <- main()
#   
#   # Access data like this:
#   # Long format (stacked by participant): results$long_format$T001
#   # Wide format (side by side): results$wide_format$T001
#   
#   # Optional: Save to CSV
#   # save_trial_data(results$long_format, "output/long_format", "long")
#   # save_trial_data(results$wide_format, "output/wide_format", "wide")
# }
# 
# results$wide_format$T001

# Function to extract only position columns for a trial
extract_position_columns <- function(trial_df_wide) {
  trial_df_wide %>%
    select(matches("_pos_x$|_pos_z$"))
}

# Or if you want to keep them organized by participant
extract_position_columns_organized <- function(trial_df_wide) {
  # Get all column names
  all_cols <- names(trial_df_wide)
  
  # Extract participant_session combinations
  participants <- unique(str_extract(all_cols, "^[^_]+_[^_]+"))
  participants <- participants[!is.na(participants)]
  
  # Select columns in organized order (pos_x, pos_z for each participant)
  selected_cols <- c()
  for (p in participants) {
    pos_x_col <- paste0(p, "_pos_x")
    pos_z_col <- paste0(p, "_pos_z")
    if (pos_x_col %in% all_cols) selected_cols <- c(selected_cols, pos_x_col)
    if (pos_z_col %in% all_cols) selected_cols <- c(selected_cols, pos_z_col)
  }
  
  trial_df_wide %>%
    select(all_of(selected_cols))
}

results <- main()
