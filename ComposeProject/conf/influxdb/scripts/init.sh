#!/bin/bash
set -e

influx bucket create -n history_structure_controller -r 0
influx bucket create -n history_structure_mineral -r 0
influx bucket create -n history_structure_deposit -r 0
influx bucket create -n history_structure_wall -r 0
influx bucket create -n history_structure_constructionsite -r 0
influx bucket create -n history_structure_container -r 0
influx bucket create -n history_structure_extension -r 0
influx bucket create -n history_structure_extractor -r 0
influx bucket create -n history_structure_factory -r 0
influx bucket create -n history_structure_invadercore -r 0
influx bucket create -n history_structure_keeperlair -r 0
influx bucket create -n history_structure_lab -r 0
influx bucket create -n history_structure_link -r 0
influx bucket create -n history_structure_observer -r 0
influx bucket create -n history_structure_portal -r 0
influx bucket create -n history_structure_powerbank -r 0
influx bucket create -n history_structure_powerspawn -r 0
influx bucket create -n history_structure_rampart -r 0
influx bucket create -n history_structure_road -r 0
influx bucket create -n history_structure_ruin -r 0
influx bucket create -n history_structure_source -r 0
influx bucket create -n history_structure_spawn -r 0
influx bucket create -n history_structure_storage -r 0
influx bucket create -n history_structure_terminal -r 0
influx bucket create -n history_structure_tombstone -r 0
influx bucket create -n history_structure_nuker -r 0
influx bucket create -n history_structure_nuke -r 0
influx bucket create -n history_creep_owned -r 0
influx bucket create -n history_creep_enemy -r 0
influx bucket create -n history_creep_other -r 0
influx bucket create -n history_creep_power -r 0
influx bucket create -n history_groundresource -r 0
